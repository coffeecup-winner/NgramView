using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NgramView.Controls.ViewInfo {
    public class BaseViewInfo {
        readonly BaseViewInfo parent;
        readonly List<BaseViewInfo> children = new List<BaseViewInfo>();
        int left, top;
        int width, height;
        Padding margin = new Padding(0);

        public BaseViewInfo() : this(null) { }
        public BaseViewInfo(BaseViewInfo parent) {
            this.parent = parent;
        }
        public int Left { get { return left; } set { left = value; } }
        public int Top { get { return top; } set { top = value; } }
        public int Right { get { return Left + Width; } }
        public int Bottom { get { return Top + Height; } }
        public int Width { get { return width; } set { width = value; } }
        public int Height { get { return height; } set { height = value; } }
        public Padding Margin {
            get { return margin; }
            set {
                if(margin == value) return;
                margin = value;
                ParentSizeChanged();
            }
        }
        protected BaseViewInfo Root { get { return Parent == null ? this : Parent.Root; } }
        protected BaseViewInfo Parent { get { return parent; } }
        protected List<BaseViewInfo> Children { get { return children; } }
        internal void ParentSizeChanged() {
            Left = Parent.Left + Margin.Left;
            Top = Parent.Top + Margin.Top;
            Width = Parent.Width - (Margin.Left + Margin.Right);
            Height = Parent.Height - (Margin.Top + margin.Bottom);
            OnParentSizeChanged();
            foreach(var child in Children)
                child.ParentSizeChanged();
        }
        protected virtual void OnParentSizeChanged() { }
        internal void Draw(PaintEventArgs e) {
            OnDraw(e);
            foreach(var child in Children)
                child.Draw(e);
        }
        protected virtual void OnDraw(PaintEventArgs e) { }
        public virtual void Update() { }
    }
}
