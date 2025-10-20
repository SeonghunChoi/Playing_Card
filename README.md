# Playing Card

게임 구성 데이터를 조합하여 룰이 다른 포커 게임들을 구현한다.

---


## Unity Version

Unity 6000.2.6f2

---

## 개발 동기
서비스 중인 게임에 미니게임으로 플레잉 카드를 활용하는 게임을 만들 때 
비슷한 룰의 게임을 매번 새로 만드는 것이 시간이 매우 아깝다는 생각이 들어 
설정만으로도 게임룰이 다르게 적용되는 플레잉카드 전용 로직이 있으면 좋겠다는 생각을 하게 되었다.

데이터는 작성하기 쉬운 ScriptableObject 로 만들고 
각 데이터를 조합하여 포커 게임 한 가지를 구성한다.
룰이 다른 새로운 포커를 추가할 경우 코드를 변경하는것이 아닌 
구성데이터를 변경하여 새로운 포커게임을 추가 할 수 있다면, 상당히 편할 것 같았다.

---

## 개발 목표
VContainer와 MessagePipe 만을 이용하여 DI 를 최대한 사용하는 구조로 개발한다.
+ 독립적으로 분리 가능한 부분은 Assembly Definition 을 이용하여 최대한 분리한다.
+ GamePlay 영역은 MVP를 최대한 지향한다.
+ 설정 데이터를 구성하고 Vcontainer 를 활용해 게임을 초기화하고 실행한다.
+ 설정 데이터만을 이용해 텍사스 홀덤을 구현한다.
+ 설정 데이터만을 이용해 세븐 스터드를 구현한다.
+ 설정 데이터만을 이용해 파이브 카드 드로우를 구현한다.

---

## 게임 구성 방법
아래 ScriptableObject 를 조합하여 포커게임을 구성 할 수 있다.

### Game
+ Create>GameData>Game 로 생성 가능
+ Main Menu 에서 게임 이름과 설명을 설정할 수 있고, Game에서 사용할 카드 정보(GameDeck) 룰 정보(GameRule)를 설정한다.
+ GameDeck, GameRule 은 필수 정보이다.

### GameDeck
+ Create>GameData>Deck 로 생성 가능
+ 게임에서 사용 할 카드들의 정보를 설정 할 수 있다. WildCardCount 로 덱에서 사용할 Wild Card 장수를 정한다.
+ GameSuits 를 추가해 Wild Card 가 아닌 일반 카드들을 구성 할 수 있다.

### GameSuits
+ Create>GameData>Suit 로 생성 가능
+ Wild Card를 제외한 카드를 설정할 수 있다.
+ SuitType 에서 Spades, Hearts, Diamonds, Clubs 을 정할 수 있다.
+ Multiply 에서 이 Suit 의 중복 벌 수를 정할 수 있다. 예를 들어 2를 입력하면 지정한 카드들이 각각 2장씩 들어간다.
+ MinValue, MaxValue 를 이용해 Deck에 등록할 카드의 범위를 지정한다.
+ 1, 14 는 어떤 값이 들어가든 Ace가 등록된다. 둘다 있어도 등록되는 것은 1장이다.

### GameRule
+ Create>GameData>Rule 로 생성 가능
+ 게임 시작 최소 인원과 최대 인원 설정, 최소 Raise 금액 설정.
+ 게임 라운드별 정보(GameRound)를 설정한다. 

### GameRound
+ Create>GameData>Round 로 생성 가능
+ RoundName 을 비워두면 GameRule 에 등록된 순서대로 번호로 표시된다.
+ Blind 해당 라운드에서 Blind 베팅 규칙을 적용한다.
+ Ante 해당 라운드에서 Ante를 적용한다.
+ BurnCardCount 는 해당 라운드 시작시 Burn을 적용할 카드 수이다.
+ DrawCardCount 는 해당 라운드에서 카드를 받은 후 한 번에 Draw를 할 수 있는 최대 카드 수 이다.
+ DealCards 에 등록된 DealCardInfo 개수 만큼 카드를 나눠준다.

### DealCardInfo
+ Create>GameData>DealCard 로 생성 가능
+ DealTarget 은 카드를 나눠줄 대상을 정한다. Table 로 하면 ComunityCard 로 사용된다.
+ DealFace 는 나눠주는 카드의 공개 여부이다. FaceDown으로 주면 받은 사람만 확인 가능하다.
Table이 대상일 경우 DealFace는 관계없이 공개 한다.

---

## 게임 등록 방법
위에서 구성한 게임을 StartUp Scene 에있는 ApplicationController 에 등록한다.

---

## 플레이 방법
아직 멀티 플레이 기능이 구현되지 않았고, AI도 추가되지 않은 상태라 각 플레이어 별로 Turn을 돌아가며 플레이 한다.
TurnAction을 하면, 카메라가 이동하며 각 플레어의 Hand를 보여준다. 자신이 현재 할 수 있는 액션이 좌측 하단에 보여진다.

---

## 추후 목표
### Betting Rule 추가
+ 현재는 Betting을 별도의 설정에 따라 진행하는게 아니라 텍사스 홀덤의 베팅규칙을 동일하게 적용하고 있다. 
이 부분도 설정으로 적용할 수 있게 하고 싶다.

### Multiplay 추가
+ Netcode 를 활용한 Mubliplay 기능 추가.

---
