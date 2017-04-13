using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Vision.v1;
using Google.Apis.Services;
using Google.Apis.Vision.v1.Data;

namespace GoogleVisionAPI_Test
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void btnImg_Click(object sender, EventArgs e)
		{
			//구글 api 자격증명
			GoogleCredential credential = null;

			//다운받은 '사용자 서비스 키'를 지정하여 자격증명을 만듭니다.
			using (var stream = new FileStream(Application.StartupPath + "\\My Project.json", FileMode.Open, FileAccess.Read))
			{
				string[] scopes = { VisionService.Scope.CloudPlatform };
				credential = GoogleCredential.FromStream(stream);
				credential = credential.CreateScoped(scopes);
			}

			//자격증명을 가지고 구글 비전 서비스를 생성합니다.
			var service = new VisionService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "google vision",
				GZipEnabled = true,
			});


			service.HttpClient.Timeout = new TimeSpan(1, 1, 1);
			//이미지를 읽어 들입니다.
			byte[] file = File.ReadAllBytes(@"aaa.png");

			//분석 요청 생성
			BatchAnnotateImagesRequest batchRequest = new BatchAnnotateImagesRequest();
			batchRequest.Requests = new List<AnnotateImageRequest>();
			batchRequest.Requests.Add(new AnnotateImageRequest()
			{
				//"TEXT_DETECTION"로 설정하면 이미지에 텍스트만 추출 합니다.
				Features = new List<Feature>() { new Feature() { Type = "TEXT_DETECTION", MaxResults = 1 }, },
				ImageContext = new ImageContext() { LanguageHints = new List<string>() { "en" } },
				Image = new Google.Apis.Vision.v1.Data.Image() { Content = Convert.ToBase64String(file) }
			});


			var annotate = service.Images.Annotate(batchRequest);
			//요청 결과 받기
			BatchAnnotateImagesResponse batchAnnotateImagesResponse = annotate.Execute();
			if (batchAnnotateImagesResponse.Responses.Any())
			{
				AnnotateImageResponse annotateImageResponse = batchAnnotateImagesResponse.Responses[0];
				if (annotateImageResponse.Error != null)
				{//에러
					if (annotateImageResponse.Error.Message != null)
						textBox1.Text = annotateImageResponse.Error.Message;
				}
				else
				{//정상 처리
					textBox1.Text = annotateImageResponse.TextAnnotations[0].Description.Replace("\n", "\r\n");
				}

			}

		}


	}
}
