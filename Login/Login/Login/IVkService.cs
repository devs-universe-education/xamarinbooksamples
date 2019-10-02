using System.Threading.Tasks;

namespace Login
{
    public interface IVkService
    {
        Task<LoginResult> Login();
        void Logout();
    }
}
