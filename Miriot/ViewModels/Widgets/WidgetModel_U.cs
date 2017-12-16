using Miriot.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public abstract class WidgetModel<U> : WidgetModel
    {
        private U _model;

        public U Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public WidgetModel(Widget widgetEntity) : base(widgetEntity)
        {
            if (widgetEntity == null) return;

            X = widgetEntity.X;
            Y = widgetEntity.Y;
            _infos = widgetEntity.Infos;
        }

        public override Widget ToWidget()
        {
            var infos = GetModel();
            return new Widget
            {
                Id = Guid.NewGuid(),
                Type = Type,
                X = X,
                Y = Y,
                Infos = infos == null ? null : JsonConvert.SerializeObject(infos)
            };
        }

        public override Task Load()
        {
            if (!string.IsNullOrEmpty(_infos))
                _model = JsonConvert.DeserializeObject<U>(_infos);

            return Task.FromResult(0);
        }

        public new virtual U GetModel()
        {
            return _model;
        }
    }
}
