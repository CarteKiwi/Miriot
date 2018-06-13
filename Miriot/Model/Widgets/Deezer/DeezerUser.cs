namespace Miriot.Common.Model.Widgets
{
	public class DeezerUser
	{
		/// <summary>Gets or sets user Id.</summary>
		public string Id { get; set; }

		/// <summary>Gets or sets user name.</summary>
		public string Name { get; set; }

		public string Code { get; set; }

		public string Token { get; set; }

		/// <summary>Gets or sets user photo.</summary>
		public byte[] Photo { get; set; }
	}
}
