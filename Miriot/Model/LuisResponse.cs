using Miriot.Common.Model;
using System.Collections.Generic;

namespace Miriot.Common
{
    public class LuisResponse
    {
        public string Query { get; set; }

        public List<IntentResponse> Intents { get; set; }

        public object Entities { get; set; }
    }

    public class IntentResponse
    {
        public string Intent { get; set; }
        public float Score { get; set; }

        public List<Action> Actions { get; set; }
    }

    public class Action
    {
        public bool Triggered { get; set; }
        public string Name { get; set; }
        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public List<ParameterValue> Value { get; set; }
    }

    public class ParameterValue
    {
        public string Entity { get; set; }
        public string Type { get; set; }
        public float Score { get; set; }
    }
}