﻿using ReactiveUI;

namespace Sma5hMusic.GUI.ViewModels
{
    public class LocaleViewModel : ReactiveObject
    {
        public bool AllFlag { get; }
        public string Id { get; }
        public string Label { get; }

        public LocaleViewModel(string id, string label, bool allFlag = false)
        {
            Id = id;
            Label = label;
            AllFlag = allFlag;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is LocaleViewModel p))
                return false;

            return p.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
