
namespace picibird.hbs.viewmodels.osk
{
    public interface IOsk
    {

        bool IsEnabled { get; set; }
        bool isOpen();
        void open();
        void close();
        void toggle();
    }
}
