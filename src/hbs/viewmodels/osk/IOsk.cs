
namespace picibird.hbs.viewmodels.osk
{
    public interface IOsk
    {

        bool IsEnabled { get; set; }
        bool isOpen();
        void open(bool force = false);
        void close(bool force = false);
        void toggle();
    }
}
