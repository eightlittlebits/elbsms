using System.ComponentModel;
using System.Windows.Forms;

namespace elb_utilities.Components
{
    // https://social.msdn.microsoft.com/Forums/windows/en-US/0b8cba1e-f7ce-4ab0-a45b-2093dc38afc8/bind-property-in-toolstripmenuitem
    public class BindableMenuItem : MenuItem, IBindableComponent
    {
        private BindingContext _bindingContext;
        private ControlBindingsCollection _dataBindings;

        [Browsable(false)]
        public BindingContext BindingContext { get => _bindingContext ?? (_bindingContext = new BindingContext()); set => _bindingContext = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ControlBindingsCollection DataBindings => _dataBindings ?? (_dataBindings = new ControlBindingsCollection(this));
    }

}
