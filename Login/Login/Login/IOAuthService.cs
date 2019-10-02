using System.Threading.Tasks;

namespace Login
{
    public interface IOAuthService
    {
        Task<LoginResult> Login();
        void Logout();
    }
}