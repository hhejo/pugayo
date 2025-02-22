# 퍼가요 (PuGaYo)

## 요약

> C#을 이용한 구글 검색 윈도우 데스크탑 앱

![퍼가요 (PuGaYo)](./assets/000-pugayo-main.png)

## 상세

6번째 PJT

### 0. 목차

1. 소개
2. 기술 스택
3. 느낀 점
4. 기능 (페이지 구성)
5. 아쉬웠던 부분
6. 앞으로 학습할 것들, 나아갈 방향
7. 어려웠던 부분, 해결한 과정

### 1. 소개

**퍼가요 (PuGaYo)**

- `C#`을 이용한 구글 검색 윈도우 데스크탑 앱
- `C#`, `.NET`, `Windows Forms`로 구성
- 구글 검색을 했을 때, 페이지를 넘겨 가며 보는 것은 번거로움
- 한번에 결과를 확인하고 싶을 때 사용할 수 있는 애플리케이션
- `PuppeteerSharp`를 이용해 구글 검색 결과를 가져옴
- 리스트 뷰에서 클릭해 해당 링크로 자동 이동
- 검색 결과는 CSV 파일로 저장할 수도 있음

작업 기간

- 2024/11, 1주

인력 구성

- 1인

### 2. 기술 스택

<img src="https://img.shields.io/badge/c%23-68217A?style=for-the-badge&logo=csharp&logoColor=black"> <img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=.NET&logoColor=white">

### 3. 느낀 점

- `Java`와 비슷하다는 `C#`을 처음 배웠는데, `Java`도 배워본 적이 없어 까다로웠다
- 프로젝트를 진행하며 `C#`은 좋은 언어라는 생각이 들었다
- `Windows Forms`를 이용해 데스크탑 앱을 간단하고 쉽게 만들 수 있어 신기했다

### 4. 기능 (페이지 구성)

1. 입력창, 버튼
2. 상태창
3. CSV 저장 버튼
4. 프로그레스 바
5. 검색어 표시
6. 검색 결과 리스트 뷰

|                                                       |                                                        |                                                       |
| :---------------------------------------------------: | :----------------------------------------------------: | :---------------------------------------------------: |
|   초기 화면 ![초기 화면](./assets/01-초기화면.png)    | 검색어 입력 ![검색어 입력](./assets/02-검색어입력.png) |  검색 실행 ![검색 실행 1](./assets/03-검색실행1.png)  |
| 검색 실행 2 ![검색 실행 2](./assets/04-검색실행2.png) |    검색 성공 ![검색 성공](./assets/05-검색성공.png)    | 결과 출력 1 ![결과 출력 1](./assets/06-결과출력1.png) |
| 결과 출력 2 ![결과 출력 2](./assets/07-결과출력2.png) |    출력 완료 ![출력 완료](./assets/08-출력완료.png)    |   결과 저장 ![결과 저장](./assets/10-결과저장.PNG)    |

아이템 클릭 (해당 링크로 이동)

![아이템 클릭](./assets/09-아이템클릭.PNG)

시연 영상

- `assets` 폴더에 위치

### 5. 아쉬웠던 부분

- 프로젝트 중 다른 일로 인해 많은 시간을 쏟을 수 없었음
- `C#` 문법에 익숙하지 않아 코드가 길어져서 아쉬움
- `Windows Forms`로 작업하는 것이 처음이라, 기능 구현을 우선하고 디자인은 신경쓰지 않은 점이 아쉬움
- 기능을 좀 더 함수로 나누고 관리하고 싶었는데, 한 줄로 끝나는 부분이 많아 애매했음
- UI 관리 부분과 기능 관리 부분을 함수로 잘 나눴어야 하는데 그러지 못해 아쉬움

### 6. 앞으로 학습할 것들, 나아갈 방향

- `C#`에 흥미가 생겨 문법을 더 공부하는 중
- `ASP .NET Core`에도 관심이 있어 가벼운 서버 프로젝트를 진행할 예정

### 7. 어려웠던 부분, 해결한 과정

`async`, `await`

- `C#`에서 비동기 프로그래밍을 어떻게 하는지 몰랐는데, `JavaScript`와 비슷한 부분이 있어서 바로 적응할 수 있었음
- 함수로 분리할 때 `Task`로 표현하는 것 같음

`PuppeteerSharp`

- `Node.js`로 크롤링을 위해 `Puppeteer`를 사용해봤었는데, `C#` 버전도 있어서 반가웠음

기타

- `var` 사용이 `JavaScript`와 다르면서 더 편리했음
- 네임스페이스, 클래스, 메서드가 정말 다양해서 복잡하기도 하고 편리하기도 했음
- `LINQ`라는 것을 알게 되고 약간 사용해봤는데, `C#`에서 사용할 수 있는 편리한 기능이었음
