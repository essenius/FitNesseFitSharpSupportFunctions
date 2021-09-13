using SupportFunctions.Model;

namespace SupportFunctionsTest
{
    class RegistryWrapperMock : RegistryWrapper
    {
        public override string ShortDateFormat => "dd-MMM-yyyy";
        public override string TimeFormat => "HH:mm:ss";
    }
}
