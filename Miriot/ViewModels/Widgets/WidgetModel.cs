using Miriot.Common.Model;
using Miriot.Resources;
using Miriot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public abstract class WidgetModel : INotifyPropertyChanged
    {
        #region Variables
        private int? _x;
        private int? _y;
        private int _position;
        private string _title;
        private bool _isActive;
        protected string _infos;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public List<string> States => Enum.GetNames(typeof(WidgetStates)).Select(e => Strings.ResourceManager.GetString(e)).ToList();

        public int? X
        {
            get { return _x; }
            set { Set(ref _x, value); }
        }

        public int? Y
        {
            get { return _y; }
            set { Set(ref _y, value); }
        }

        public int Position
        {
            get { return GetPositionFromXY(X, Y); }
            set
            {
                X = GetXFromIndex(value);
                Y = GetYFromIndex(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Position"));
            }
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
                {
                    X = null;
                    Y = null;
                    OnDisabled();
                }
            }
        }

        public virtual string Title
        {
            get { return string.IsNullOrEmpty(_title) ? Type.ToString() : _title; }
            set { Set(ref _title, value); }
        }

        public virtual WidgetStates State => WidgetStates.Standard;

        public abstract WidgetType Type { get; }

        public virtual bool IsBuiltIn => false;

        #endregion



        public WidgetModel(Widget widgetEntity)
        {
            if (widgetEntity == null) return;

            X = widgetEntity.X;
            Y = widgetEntity.Y;
            _infos = widgetEntity.Infos;
        }

        public virtual Widget ToWidget()
        {
            return new Widget
            {
                Id = Guid.NewGuid(),
                Type = Type,
                X = X,
                Y = Y,
                Infos = null
            };
        }

        public virtual Task Load()
        {
            return Task.FromResult(0);
        }

        private int? GetXFromIndex(int index)
        {
            switch (index)
            {
                case 1:
                case 4:
                case 7:
                    return 0;
                case 2:
                case 5:
                case 8:
                    return 1;
                case 3:
                case 6:
                case 9:
                    return 2;
                default:
                    return null;
            }
        }

        private int? GetYFromIndex(int index)
        {
            switch (index)
            {
                case 1:
                case 2:
                case 3:
                    return 0;
                case 4:
                case 5:
                case 6:
                    return 1;
                case 7:
                case 8:
                case 9:
                    return 2;
                default:
                    return null;
            }
        }

        private int GetPositionFromXY(int? X, int? Y)
        {
            if (X == null || Y == null)
                return 0;

            if (X == 0)
            {
                if (Y == 0)
                    return 1;
                if (Y == 1)
                    return 4;
                if (Y == 2)
                    return 7;
            }

            if (X == 1)
            {
                if (Y == 0)
                    return 2;
                if (Y == 1)
                    return 5;
                if (Y == 2)
                    return 8;
            }

            if (X == 2)
            {
                if (Y == 0)
                    return 3;
                if (Y == 1)
                    return 6;
                if (Y == 2)
                    return 9;
            }

            return 0;
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
