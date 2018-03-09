using System.Collections.Generic;

namespace Miriot.Common
{
    public class LuisResponse
    {
        public string Query { get; set; }

        public IntentResponse TopScoringIntent { get; set; }

        public List<LuisEntity> Entities { get; set; }
    }

    public class IntentResponse
    {
        public string Intent { get; set; }
        public float Score { get; set; }
    }

    public class LuisEntity
    {
        public string Entity { get; set; }
        public string Type { get; set; }
        public float Score { get; set; }
    }
}