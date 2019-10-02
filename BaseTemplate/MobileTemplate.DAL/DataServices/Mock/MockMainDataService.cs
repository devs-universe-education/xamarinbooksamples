using System.Threading;
using System.Threading.Tasks;
using MobileTemplate.DAL.DataObjects;

namespace MobileTemplate.DAL.DataServices.Mock {
	class MockMainDataService: BaseMockDataService, IMainDataService {
		public Task<RequestResult<SampleDataObject>> GetSampleDataObject(CancellationToken ctx) {
			return GetMockData<SampleDataObject>("MobileTemplate.DAL.Resources.Mock.Main.SampleDataObject.json");
		}
	}
}
