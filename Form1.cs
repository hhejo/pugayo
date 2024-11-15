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
            // 구글 검색 시작 전 값 초기화
            labelQuery.Text = TextBox.Text; // 입력한 검색어 출력
            TextBox.Text = ""; // 검색창 초기화
            listView.Items.Clear(); // 리스트뷰 아이템 초기화
            SetLabelStatus(""); // 상태창 초기화
            SetProgressBarValue(0); // 프로그레스바 초기화
            SetBtnSaveState(false); // 저장 버튼 비활성화
            try
            {
                SetLabelStatus("초기 설정중...");
                SetIsSearching(true); // 검색중
                IBrowser browser = await InitBrowser(); // Browser 초기화
                IPage page = await InitPage(browser, query); // Page 생성, 구글 검색 URL 연결
                searchResults = await PerformGoogleSearchAsync(page); // 구글 검색 실행하고 결과 데이터 획득
                SetIsSearching(false); // 검색 완료
                SetLabelStatus("데이터 검색 성공!");
                await Task.Delay(2000);
                int maxCount = searchResults.Count; // 검색 결과 수
                SetProgressBarMaximum(maxCount - 1); // 프로그레스바 최댓값 설정
                foreach (var (searchResult, index) in searchResults.Select((value, index) => (value, index)))
                {
                    AddListViewItem(searchResult, index); // searchResult를 리스트뷰에 아이템으로 추가
                    SetProgressBarValue(index); // 프로그레스바 표시
                    SetLabelStatus($"{index} / {maxCount}"); // 상태창 진행률 표시
                    await Task.Delay(30); // 0.03초 대기
                }
                SetBtnSaveState(true); // 저장 가능
                SetLabelStatus($"{maxCount}개 검색 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void SetIsSearching(bool isSearched) => isSearching = isSearched;

        private void SetLabelStatus(string statusMessage) => labelStatus.Text = statusMessage;

        private void SetProgressBarValue(int value) => progressBar.Value = value;

        private void SetProgressBarMaximum(int maximum) => progressBar.Maximum = maximum;

        private void SetBtnSaveState(bool isEnabled) => BtnSave.Enabled = isEnabled;

        private void AddListViewItem(Dictionary<string, string> searchResult, int currentCount)
        {
            ListViewItem listViewItem = new ListViewItem(searchResult["title"]); // 제목
            listViewItem.SubItems.Add($"{currentCount + 1}"); // 번호
            listViewItem.SubItems.Add(searchResult["link"]); // 링크
            listView.Items.Add(listViewItem); // 리스트 뷰에 아이템 추가
        }

        // 구글 검색 실행
        private async Task<List<Dictionary<string, string>>> PerformGoogleSearchAsync(IPage page)
        {
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>(); // 검색 결과들의 리스트
            var (currentPage, hasNextPage) = (1, true); // 현재 페이지, 다음 페이지 보유 여부
            while (hasNextPage)
            {
                SetLabelStatus(labelStatus.Text.Length > 12 ? "데이터 검색중" : labelStatus.Text + '.');
                string expression = @"Array.from(document.querySelectorAll('div h3')).map(h3 => h3.innerText);";
                var texts = await page.EvaluateExpressionAsync<List<string>>(expression); // h3 태그 내용 획득
                expression = @"Array.from(document.querySelectorAll('a h3')).map(h3 => h3.parentElement.href);";
                var hrefs = await page.EvaluateExpressionAsync<List<string>>(expression); // a 태그 href 획득
                for (int i = 0; i < texts.Count && i < hrefs.Count; i++)
                {
                    var item = new Dictionary<string, string> {{ "title", texts[i] }, { "link", hrefs[i] }};
                    results.Add(item);
                }
                expression = @"document.getElementById('pnnext') !== null";
                hasNextPage = await page.EvaluateExpressionAsync<bool>(expression); // 다음 페이지 탐색
                if (!hasNextPage)
                {
                    break; // 다음 페이지가 없으면 종료
                }
                await page.ClickAsync("#pnnext"); // 다음 페이지 이동 클릭
                await page.WaitForNavigationAsync(); // 이동한 페이지 대기
                await Task.Delay(new Random().Next(200, 300));
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

        // 구글 검색 준비
        private async Task<IPage> InitPage(IBrowser browser, string query)
        {
            var page = await browser.NewPageAsync(); // Page 생성
            string searchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}"; // 구글 검색 URL
            await page.GoToAsync(searchUrl); // 해당 URL로 이동
            return page;
        }

        private void listView_ItemActivate(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count <= 0)
            {
                return;
            }
            var selectedItem = listView.SelectedItems[0]; // 클릭한 아이템
            string link = selectedItem.SubItems[2].Text; // 해당 아이템의 링크
            try
            {
                System.Diagnostics.Process.Start("chrome.exe", link); // 해당 링크로 크롬 실행
                selectedItem.BackColor = Color.SkyBlue;
                selectedItem.ForeColor = Color.White;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"크롬 실행 오류: {ex}");
            }
        }

        // 결과를 CSV 파일로 저장
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

        // 저장 버튼 클릭
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
