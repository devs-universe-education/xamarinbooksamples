using System;

namespace MobileTemplate.DAL.DataServices
{
	public static class DataServices {
		public static IMainDataService Main { get; private set; }

		public static void Init(bool isMock) {
			if (isMock) {
				Main = new Mock.MockMainDataService();
			}
			else {
				throw new NotImplementedException("Online Data Services not implemented");
			}
		}
	}
}
