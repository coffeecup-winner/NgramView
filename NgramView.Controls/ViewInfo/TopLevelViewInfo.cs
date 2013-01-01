using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NgramView.Controls.ViewInfo {
    public class TopLevelViewInfo<TControl> : BaseViewInfo
    where TControl : Control {
        readonly TControl owner;

        public TopLevelViewInfo(TControl owner) {
            this.owner = owner;
            SubscribeEvents();
        }
        public TControl Owner { get { return owner; } }
        void SubscribeEvents() {
            Owner.LocationChanged += (s, e) => OnOwnerLocationChanged();
            Owner.SizeChanged += (s, e) => OnOwnerSizeChanged();
        }
        void OnOwnerLocationChanged() {
            
        }
        void OnOwnerSizeChanged() {
            Width = Owner.Width;
            Height = Owner.Height;
            foreach(var child in Children)
                child.ParentSizeChanged();
        }
    }
}
