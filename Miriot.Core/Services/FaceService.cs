using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using Rectangle = Microsoft.ProjectOxford.Common.Rectangle;

namespace Miriot.Core.Services
{
    public class FaceService : IFaceService
    {
        private const string OxfordFaceKey = "89b53f9d8db046ae9fca71ee98a72330";
        private const string OxfordEmotionKey = "5194871378f6446c91a6a247495cb6f5";

        private string _miriotPersonGroupId;

        public FaceService()
        {
            Task.Run(async () => await LoadGroup());
        }

        private async Task LoadGroup()
        {
            try
            {
                var faceClient = new FaceServiceClient(OxfordFaceKey);

                // Récupère les groupes
                var groups = await faceClient.ListPersonGroupsAsync();

                // Récupère l'id du premier groupe
                _miriotPersonGroupId = groups.First().PersonGroupId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task<ServiceResponse> GetUsers(byte[] bitmap)
        {
            var faceClient = new FaceServiceClient(OxfordFaceKey);

            List<Face> faces;
            Debug.WriteLine("DetectAsync started");
            // Détection des visages sur la photo
            using (var stream = new MemoryStream(bitmap))
                faces = (await faceClient.DetectAsync(stream)).ToList();
            Debug.WriteLine("DetectAsync ended");

            // Récupération des identifiants Oxford
            var faceIds = faces.Select(o => o.FaceId).ToList();

            if (!faceIds.Any())
                return new ServiceResponse(null, ErrorType.NoFaceDetected);
            Debug.WriteLine("IdentityAsync started");

            // Identification des personnes à partir des visages
            var identifyResults = (await faceClient.IdentifyAsync(_miriotPersonGroupId, faces.Select(o => o.FaceId).ToArray())).ToList();
            Debug.WriteLine("IdentityAsync ended");

            // Visage inconnu du groupe
            if (identifyResults.Count == 0 || !identifyResults.Any(o => o.Candidates.Any()))
                return new ServiceResponse(null, ErrorType.UnknownFace);

            // Une fois les personnes identifiées, on ne garde que les 2 mieux reconnues
            var mostConfidentPersons = identifyResults.SelectMany(p => p.Candidates).OrderByDescending(o => o.Confidence).Take(2);
            Debug.WriteLine("GetPerson started");

            var users = new List<User>();
            foreach (var mostConfidentPerson in mostConfidentPersons)
            {
                var person = await faceClient.GetPersonAsync(_miriotPersonGroupId, mostConfidentPerson.PersonId);
                Debug.WriteLine("GetPerson ended");

                var faceId = identifyResults.First(r => r.Candidates.Select(c => c.PersonId).Contains(mostConfidentPerson.PersonId)).FaceId;
                var face = faces.Single(o => o.FaceId == faceId);

                var data = TryDeserialize(person.UserData);

                var user = new User
                {
                    Id = person.PersonId,
                    Name = person.Name,
                    UserData = data,
                    FaceRectangle = new Rectangle
                    {
                        Height = face.FaceRectangle.Height,
                        Width = face.FaceRectangle.Width, 
                        Top = face.FaceRectangle.Top,
                        Left = face.FaceRectangle.Left
                    },
                    Picture = bitmap
                };

                users.Add(user);
            }

            return new ServiceResponse(users.ToArray(), null);
        }

        private UserData TryDeserialize(string userData)
        {
            try
            {
                return JsonConvert.DeserializeObject<UserData>(userData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("UserData:" + userData);
                return new UserData();
            }
        }

        public async Task<UserEmotion> GetEmotion(byte[] bitmap, int top, int left)
        {
            try
            {
                List<Emotion> emotions;
                var emotionClient = new EmotionServiceClient(OxfordEmotionKey);

                using (var stream = new MemoryStream(bitmap))
                    emotions = (await emotionClient.RecognizeAsync(stream)).ToList();

                var selectedEmotion = emotions.SingleOrDefault(o => o.FaceRectangle.Top == top && o.FaceRectangle.Left == left);

                if (selectedEmotion != null)
                {
                    var emotion = new Dictionary<UserEmotion, float>
                        {
                            {UserEmotion.Anger, selectedEmotion.Scores.Anger},
                            {UserEmotion.Contempt, selectedEmotion.Scores.Contempt},
                            {UserEmotion.Disgust, selectedEmotion.Scores.Disgust},
                            {UserEmotion.Fear, selectedEmotion.Scores.Fear},
                            {UserEmotion.Happiness, selectedEmotion.Scores.Happiness},
                            {UserEmotion.Neutral, selectedEmotion.Scores.Neutral},
                            {UserEmotion.Sadness, selectedEmotion.Scores.Sadness},
                            {UserEmotion.Surprise, selectedEmotion.Scores.Surprise}
                        };

                    return emotion.OrderByDescending(o => o.Value).First().Key;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return UserEmotion.Unknown;
        }

        public async Task<bool> DeletePerson(Guid personId)
        {
            try
            {
                var faceClient = new FaceServiceClient(OxfordFaceKey);
                await faceClient.DeletePersonAsync(_miriotPersonGroupId, personId);
                await faceClient.TrainPersonGroupAsync(_miriotPersonGroupId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CreatePerson(byte[] bitmap, string name)
        {
            try
            {
                var faceClient = new FaceServiceClient(OxfordFaceKey);

                // Create PERSON
                var newPerson = await faceClient.CreatePersonAsync(_miriotPersonGroupId, name, JsonConvert.SerializeObject(new UserData()));

                // Add FACE to PERSON
                using (var stream = new MemoryStream(bitmap))
                    await faceClient.AddPersonFaceAsync(_miriotPersonGroupId, newPerson.PersonId, stream);

                await faceClient.TrainPersonGroupAsync(_miriotPersonGroupId);

                // TODO: Add a loop to wait until Training is done.

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePerson(User user, byte[] pic)
        {
            try
            {
                var faceClient = new FaceServiceClient(OxfordFaceKey);

                // Update user's data
                await faceClient.UpdatePersonAsync(_miriotPersonGroupId, user.Id, user.Name, JsonConvert.SerializeObject(user.UserData));

                // Add the new face
                using (var stream = new MemoryStream(pic))
                    await faceClient.AddPersonFaceAsync(_miriotPersonGroupId, user.Id, stream);

                // Train model
                await faceClient.TrainPersonGroupAsync(_miriotPersonGroupId);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateUserDataAsync(User user)
        {
            try
            {
                var faceClient = new FaceServiceClient(OxfordFaceKey);

                // Update user's data
                await faceClient.UpdatePersonAsync(_miriotPersonGroupId, user.Id, user.Name, JsonConvert.SerializeObject(user.UserData));

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
