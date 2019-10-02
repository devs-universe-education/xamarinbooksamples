using System.Threading.Tasks;

namespace Login
{
    public interface IFacebookService
    {
        Task<LoginResult> Login();
        void Logout();
    }
}
