using Miriot.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public abstract class WidgetModel<U> : INotifyPropertyChanged
    {
        #region Variables
        private int _x;
        private int _y;
        private string _title;
        private bool _isActive;
        protected List<string> _infos;
        private U _model;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public U Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public int X
        {
            get { return _x; }
            set { Set(ref _x, value); }
        }

        public int Y
        {
            get { return _y; }
            set { Set(ref _y, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                Set(ref _isActive, value);
                if (value)
                    OnActivated();
                else
                    OnDisabled();
            }
        }

        public virtual string Title
        {
            get { return string.IsNullOrEmpty(_title) ? Type.ToString() : _title; }
            set { Set(ref _title, value); }
        }

        public abstract WidgetType Type { get; }

        #endregion

        public WidgetModel(Widget widgetEntity)
        {
            if (widgetEntity == null) return;

            X = widgetEntity.X;
            Y = widgetEntity.Y;
            _infos = widgetEntity.Infos;
        }

        public Widget ToWidget()
        {
            var infos = GetModel();
            return new Widget
            {
                Id = Guid.NewGuid(),
                Type = Type,
                X = X,
                Y = Y,
                Infos = infos == null ? null : new List<string> { JsonConvert.SerializeObject(infos) }
            };
        }

        public virtual Task Load()
        {
            var info = _infos?.FirstOrDefault();
            if (string.IsNullOrEmpty(info) || info == "null") return Task.FromResult(0);

            _model = JsonConvert.DeserializeObject<U>(info);

            return Task.FromResult(0);
        }

        public virtual U GetModel()
        {
            return _model;
        }

        public void SetActive()
        {
            Set(ref _isActive, true);
        }

        public virtual void OnActivated() { }
        public virtual void OnDisabled() { }

        protected void Set<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (field == null || !field.Equals(value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
