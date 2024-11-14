using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuppeteerSharp;

namespace PuGaYo
{
    public partial class Form1 : Form
    {
        private bool isSearching = false; // 현재 구글 검색중인지
        private List<Dictionary<string, string>> searchResults; // 검색 결과 딕셔너리의 리스트

        public Form1()
        {
            InitializeComponent();
        }

        // 검색창 엔터 입력
        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 엔터키를 누르지 않았거나, 현재 구글 검색중인 경우
            if (e.KeyCode != Keys.Enter || isSearching)
            {
                return;
            }

            // 검색어를 입력하지 않은 경우
            string query = TextBox.Text; // 검색어
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            InitBeforeSearch();

            try
            {
                searchResults = await PerformGoogleSearchAsync(query);
                labelStatus.Text = "드라이버 설정 성공!";
                await Task.Delay(1500);
                int currentCount = 0;
                int maxCount = searchResults.Count;
                progressBar.Maximum = maxCount - 1;
                foreach (var searchResult in searchResults)
                {
                    ListViewItem listViewItem = new ListViewItem(searchResult["title"]); // 제목
                    listViewItem.SubItems.Add($"{currentCount + 1}"); // 번호
                    listViewItem.SubItems.Add(searchResult["link"]); // 링크
                    listView.Items.Add(listViewItem); // 리스트뷰에 아이템 추가
                    progressBar.Value = currentCount;
                    labelStatus.Text = $"{currentCount} / {maxCount}";
                    currentCount++;
                    await Task.Delay(new Random().Next(20, 30));
                }
                labelStatus.Text = $"{maxCount}개 검색 완료";
                BtnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                isSearching = false;
            }
        }

        // 구글 검색 시작 전 값 초기화
        private void InitBeforeSearch()
        {
            isSearching = true; // 검색중
            labelQuery.Text = TextBox.Text; // 현재 검색어를 출력
            TextBox.Text = "";
            labelStatus.Text = ""; // 상태창
            progressBar.Value = 0; // 프로그레스바 값 초기화
            listView.Items.Clear(); // 리스트뷰 아이템 초기화
            BtnSave.Enabled = false; // 저장 버튼 비활성화
        }

        // 구글 검색
        private async Task<List<Dictionary<string, string>>> PerformGoogleSearchAsync(string query)
        {
            labelStatus.Text = "초기 설정중...";
            // Browser 초기화
            IBrowser browser = await InitBrowser();
            // Page 생성
            var page = await browser.NewPageAsync();
            string searchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}"; // 검색 URL
            await page.GoToAsync(searchUrl); // 해당 URL로 이동
            // 크롤링 준비
            List<string> titles = new List<string>();
            List<string> links = new List<string>();
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();
            int currentPage = 1;
            int periodCount = 0;
            bool hasNextPage = true;
            while (hasNextPage)
            {
                labelStatus.Text = $"드라이버 설정중{string.Concat(Enumerable.Repeat(".", periodCount))}";
                periodCount = (periodCount + 1) % 5;
                // h3 태그 내용 가져오기
                string expression = @"Array.from(document.querySelectorAll('div h3')).map(h3 => h3.innerText);";
                var texts = await page.EvaluateExpressionAsync<List<string>>(expression);
                expression = @"Array.from(document.querySelectorAll('a h3')).map(h3 => h3.parentElement.href);";
                var hrefs = await page.EvaluateExpressionAsync<List<string>>(expression);
                
                for (int i = 0; i < texts.Count && i < hrefs.Count; i++)
                {
                    var item = new Dictionary<string, string> {{ "title", texts[i] }, { "link", hrefs[i] }};
                    results.Add(item);
                }

                // 다음 페이지 탐색
                expression = @"document.getElementById('pnnext') !== null";
                hasNextPage = await page.EvaluateExpressionAsync<bool>(expression);
                if (!hasNextPage)
                {
                    break;
                }
                // 다음 페이지 이동
                await page.ClickAsync("#pnnext");
                await page.WaitForNavigationAsync();
                await Task.Delay(new Random().Next(300, 500));
                currentPage++;
            }
            return results;
        }

        // Browser 초기화
        private async Task<IBrowser> InitBrowser()
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = "C:/Program Files/Google/Chrome/Application/chrome.exe"
            };
            IBrowser browser = await Puppeteer.LaunchAsync(launchOptions);
            return browser;
        }

        private void listView_ItemActivate(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                var selectedItem = listView.SelectedItems[0];
                string link = selectedItem.SubItems[2].Text;
                try
                {
                    System.Diagnostics.Process.Start("chrome.exe", link);
                    selectedItem.BackColor = Color.SkyBlue;
                    selectedItem.ForeColor = Color.White;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"크롬 실행 오류: {ex}");
                }
            }
        }

        private async Task SaveResultsToCSV(List<Dictionary<string, string>> searchResults, string filepath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filepath, false, Encoding.UTF8))
                {
                    writer.WriteLine("번호,제목,링크");
                    int index = 1;
                    foreach (var searchResult in searchResults)
                    {
                        string title = searchResult.ContainsKey("title") ? searchResult["title"] : "";
                        string link = searchResult.ContainsKey("link") ? searchResult["link"] : "";
                        writer.WriteLine($"\"{index}\",\"{title}\",\"{link}\"");
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 저장 오류: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("CSV 파일로 저장할까요?", "CSV 파일 저장", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Save results to CSV file"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filepath = saveFileDialog.FileName;
                await SaveResultsToCSV(searchResults, filepath);
                MessageBox.Show("CSV 파일 저장 성공");
            }
        }
    }
}
