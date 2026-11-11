namespace InsuranceManagement.IntegrationTests.Helpers
{
    public abstract class IntegrationTestBase
    {
        private CustomWebApplicationFactory _factory = null!;

        protected HttpClient HttpClient { get; private set; } = null!;

        [TestInitialize]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            HttpClient = _factory.CreateClient();
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            HttpClient.Dispose();
            await _factory.DisposeAsync();
        }
    }
}
