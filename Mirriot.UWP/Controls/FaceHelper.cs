using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Mirriot.Controls
{
    public class FaceHelper
    {
        public const string KEY = "89b53f9d8db046ae9fca71ee98a72330";

        public static async Task<List<Face>> GetFaces(string imagePath)
        {
            // User already picked one image
            var imageInfo = await UIHelper.GetImageInfoForRendering(imagePath);

            var sw = Stopwatch.StartNew();

            // Call detection REST API, detect faces inside the image
            using (var fileStream = File.OpenRead(imagePath))
            {
                try
                {
                    var t = Window.Current.CoreWindow;
                    string subscriptionKey = KEY;

                    var faceServiceClient = new FaceServiceClient(subscriptionKey);
                    var faces = await faceServiceClient.DetectAsync(fileStream);

                    // Handle REST API calling error
                    if (faces == null)
                    {
                        return null;
                    }

                    var cFaces = new List<Face>();

                    // Convert detection results into UI binding object for rendering
                    foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, 300, imageInfo))
                    {
                        // Detected faces are hosted in result container, will be used in the verification later
                        cFaces.Add(face);
                    }

                    return cFaces;
                }
                catch (FaceAPIException ex)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Verify two detected faces, get whether these two faces belong to the same person
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        public static async Task<Microsoft.ProjectOxford.Face.Contract.VerifyResult> Verification(List<Face> faces1, List<Face> faces2)
        {
            // Call face to face verification, verify REST API supports one face to one face verification only
            // Here, we handle single face image only
            if (faces1 != null && faces2 != null && faces1.Count == 1 && faces2.Count == 1)
            {
                var faceId1 = faces1[0].FaceId;
                var faceId2 = faces2[0].FaceId;

                // Call verify REST API with two face id
                try
                {
                    var faceServiceClient = new FaceServiceClient(KEY);
                    return await faceServiceClient.VerifyAsync(Guid.Parse(faceId1), Guid.Parse(faceId2));
                }
                catch (FaceAPIException ex)
                {
                    return null;
                }
            }
            else
            {
                return new Microsoft.ProjectOxford.Face.Contract.VerifyResult() { IsIdentical = false };
                //var d = new MessageDialog("Verification accepts two faces as input, please pick images with only one detectable face in it.", "Warning");
                //await d.ShowAsync();
                //return null;
            }
        }

    }
}
