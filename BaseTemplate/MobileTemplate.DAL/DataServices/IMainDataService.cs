using System.Threading;
using System.Threading.Tasks;
using MobileTemplate.DAL.DataObjects;

namespace MobileTemplate.DAL.DataServices {
	public interface IMainDataService {
		Task<RequestResult<SampleDataObject>> GetSampleDataObject(CancellationToken ctx);
	}
}
