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
using PuppeteerSharp;
using System.Net.Http;

namespace PuGaYo
{
    public partial class Form1 : Form
    {
        private string imageUrl;
        private bool isCrawling = false;

        public Form1()
        {
            InitializeComponent(); // 폼 초기화
            BtnSave.Enabled = false;
        }

        private async void BtnCrawl_Click_1(object sender, EventArgs e)
        {
            if (isCrawling)
            {
                MessageBox.Show("크롤링 중입니다...");
                return;
            }
            string url = $"https://www.instagram.com/{txtUrl.Text}";
            //string url = "https://m.sports.naver.com/wfootball/article/076/0004212491";
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("계정을 입력하세요");
                isCrawling = false;
                return;
            }
            isCrawling = true;
            MessageBox.Show("크롤링을 시작합니다...");
            try
            {
                imageUrl = await CrawlImageAsync(url);
                if (string.IsNullOrEmpty(imageUrl)) MessageBox.Show("존재하지 않는 계정이거나 이미지가 없습니다.");
                else
                {
                    pictureBox.Image = await LoadImageAsync(imageUrl);
                    BtnSave.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                isCrawling = false;
            }
        }

        private async void BtnSave_Click_1(object sender, EventArgs e)
        {
            if (pictureBox.Image == null) MessageBox.Show("다운로드할 이미지가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                var filepath = Path.Combine(downloadsFolder, "downloaded_image.jpg");
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var uri = new Uri(imageUrl);
                        var imageBytes = await client.GetByteArrayAsync(uri);
                        File.WriteAllBytes(filepath, imageBytes);
                        Console.WriteLine($"이미지가 성공적으로 다운로드되었습니다: {filepath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"오류 발생: {ex.Message}");
                }
                MessageBox.Show("이미지가 성공적으로 다운로드되었습니다.", "다운로드 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task<string> CrawlImageAsync(string url)
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            var launchOptions = new LaunchOptions
            {
                ExecutablePath = "C:/Program Files/Google/Chrome/Application/chrome.exe", // Chrome 설치 경로
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-infobars",
                    "--window-position=0,0",
                    "--ignore-certificate-errors",
                    "--ignore-certificate-errors-spki-list",
                }
            };
            var browser = await Puppeteer.LaunchAsync(launchOptions);

            try
            {
                var page = await browser.NewPageAsync();
                await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>()
                {
                    {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"},
                    {"Accept-Encoding", "gzip, deflate, br"},
                    {"Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7"},
                    {"Cache-Control", "no-cache"},
                    {"Pragma", "no-cache"}
                });
                await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
                await Task.Delay(new Random().Next(3000, 7000));
                var client = await page.CreateCDPSessionAsync();
                await client.SendAsync("Network.clearBrowserCookies");
                await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
                await Task.Delay(new Random().Next(3000, 7000));
                //string selector = "#comp_news_article > div > span:nth-child(1) > span > span > img";
                await page.WaitForSelectorAsync("article img", new WaitForSelectorOptions { Timeout = 5000 });
                //const firstImage = document.querySelector('#comp_news_article > div > span:nth-child(1) > span > span > img');
                var imageUrl = await page.EvaluateExpressionAsync<string>(
                    @"(() => {
                        const firstImage = document.querySelector('article img');
                        return firstImage ? firstImage.src : '';
                    })()");
                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                await browser.CloseAsync();
            }
        }

        private async Task<Image> LoadImageAsync(string imageUrl)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                var response = await client.GetAsync(imageUrl); // 이미지 다운로드
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    return Image.FromStream(stream); // 스트림에서 이미지 로드
                }
            }
        }
    }
}
