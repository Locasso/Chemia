using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour {

    public enum Turn
    {
        PLAYER_1,
        PLAYER_2,
        PLAYER_3,
        PLAYER_4,
        GAMEOVER
    }

    public enum Phases
    {
        BEGIN,
        MIDDLE,
        END
    }

    [Header("Player no Jogo")]
    public GameObject[] players;

    [Header("Objetos")]
    public GameObject card, deck, firstInTablePosition;
    public GameObject cardInTable; //Guarda o card atual que está na mesa;
    public GameObject cardsHold; //Segura todas as cartas do jogo!

    [Header("Lista de Cards Usados")]
    public List<float> cardUsed;
    public List<GameObject> cardInDeck;
    public GameObject firstInTable;
    public int cardsInTable;

    [Header("Lista de Cards do Player")]
    public List<GameObject> playerCards;
    public List<GameObject> playerHand; //Lista de cartas na mão do jogador;

    [Header("Lista de Cards do Oponente")]
    public List<GameObject> oponentHand;

    [Header("Utilitários")]
    public float howManyAdded; // How many cards I added so far
    float gapFromOneItemToTheNextOne; //the gap I need between each card
    public int howManyToStop;
    public bool gameBegin = true; //Checa se é o começo da partida;
    public bool gameOver = false; //Checa se é o fim do jogo.
    public bool hasPlayed;

    [Header("Lógicas dos Turnos")]
    public Turn state; 
    public int idTurn = 1;
    public bool turnTheCard; //Bollean para controlar o sprite das cartas na mesa.
    public Phases phases; //Enum que controla as fases de jogo.
    public int idPhases; //Int que controla as fases do jogo.
    public bool opponentTurn;

    [Header("Lógica de Vitórias")]
    public int p1_point, p2_points;

    [Header("Feedbacks")]
    public GameObject invalidNot; //Gameobject de match inválido
    public bool invalidMatch;

    [Header("Visuais e Animações")]
    public Animator myTurn, oponentTurn;
    public GameObject victory, defeat;

    [Header("Componentes de Áudio")]
    public AudioSource audioSource;
    public AudioClip audioClip;

    void Start ()
    {
        audioSource = GetComponent<AudioSource>();
        victory.SetActive(false);
        defeat.SetActive(false);
        idTurn = Random.Range(1, 2);        
        howManyAdded = 0.0f;
        gapFromOneItemToTheNextOne = 1.0f;
        card = Resources.Load<GameObject>("Prefabs/card");
        SpawnCards(7, card);
        SpawnDeck(40, card);
        MatchsControl();
        PutInTable();
        cardInTable = firstInTable;
        cardsHold = GameObject.Find("Cards");
    }
	
	void Update ()
    {
        FitCards();
        if(!gameOver)
        TryCards();

        if(cardInDeck.Count <= 0)
        {
            SpawnDeck(40, card);
        }
        if(playerHand.Count == 0)
        {
            gameOver = true;
            victory.SetActive(true);
            state = Turn.GAMEOVER;
            cardsHold.SetActive(false);
            Debug.Log("Player Wins");
            StartCoroutine(BackToMenu());
        }
        else if(oponentHand.Count == 0)
        {
            gameOver = true;
            defeat.SetActive(true);
            state = Turn.GAMEOVER;
            cardsHold.SetActive(false);
            Debug.Log("Enemy Wins");
            StartCoroutine(BackToMenu());
        }
	}
    public void MatchsControl()
    {
        //Notificação de carta inválida
        invalidNot.SetActive(false);
        if(invalidMatch)
        {
            invalidNot.SetActive(true);
            invalidMatch = false;
        }
    }

    public void FitCards()
    {
        if (playerCards.Count == 0) //Se a lista for nula, para a função
            return;
        GameObject obj = playerCards[0]; //Referência da primeira imagem na minha lista
        obj.transform.position = players[0].transform.position;
        Vector3 handPosition = obj.transform.position;
        handPosition += new Vector3((howManyAdded * gapFromOneItemToTheNextOne), 0, 0); // Move minha carta 1f para a direita
        obj.transform.position = handPosition;
        obj.GetComponent<CardBehavior>().startPosition = handPosition;
        obj.GetComponent<CardBehavior>().isDeck = false;
        playerCards.RemoveAt(0);
        howManyAdded++;
    }

    public void SpawnCards(int numberCards, GameObject card)
    {
        for(int i = 0; i < numberCards; i++)
        {
            for(int j=0; j < players.Length; j++)
            {
                if(this.players[j].gameObject.name == "p_1")
                {
                    card.GetComponent<CardBehavior>().isPlayer = true;
                    card.GetComponent<CardBehavior>().isNotPlayer = false;               
                }
                else
                {
                    card.GetComponent<CardBehavior>().isPlayer = false;
                    card.GetComponent<CardBehavior>().isNotPlayer = true;
                    card.GetComponent<CardBehavior>().isDeck = false;

                }
                Instantiate(card, players[j].transform.position, players[j].transform.rotation);
                cardsInTable += 1;
            }
        }
    }
    public void SpawnDeck(int numberCards, GameObject card)
    {
        for (int i = 0; i < numberCards - cardsInTable; i++)
        {
                if (this.deck.gameObject.name == "deck")
                {
                    card.GetComponent<CardBehavior>().isDeck = true;
                    card.GetComponent<CardBehavior>().isNotPlayer = false;
                }
                GameObject obj = Instantiate(card, deck.transform.position, deck.transform.rotation);
                obj.name = "Card_" + i;
                cardInDeck.Add(obj);
        }
        if(gameBegin)
        PutTheStartingCard();
    }
    public void PutTheStartingCard()
    {
        firstInTable = GameObject.Find("Card_0");
        firstInTable.transform.position = firstInTablePosition.transform.position;
        firstInTable.GetComponent<CardBehavior>().isDeck = false;
        firstInTable.GetComponent<CardBehavior>().isTable = true;
        cardInDeck.Remove(firstInTable);
        turnTheCard = true;
        cardsInTable += 1;
        gameBegin = false;
    }
    public void PutInTable()//Apesar do nome, essa função controla as mensagens de passagem de turnos.
    {
        switch(idTurn)
        {
            case 1:
                myTurn.SetTrigger("Turn");
                audioClip = Resources.Load<AudioClip>("turnPass");
                audioSource.clip = audioClip;
                audioSource.Play();
                break;
            case 2:
                oponentTurn.SetTrigger("Turn");
                audioClip = Resources.Load<AudioClip>("turnPass");
                audioSource.clip = audioClip;
                audioSource.Play();
                break;
        }
    }

    public void TryCards() //Realiza diversos testes de lógica na mesa de jogo
    {
        if (this.state == Turn.PLAYER_2)
        {
            Debug.Log("Entra");
            foreach(GameObject card in oponentHand)
            {
                if (this.cardInTable.GetComponent<CardBehavior>().valorCarta == card.GetComponent<CardBehavior>().valorCarta
                    || this.cardInTable.GetComponent<CardBehavior>().corCarta == card.GetComponent<CardBehavior>().corCarta
                    || this.cardInTable.GetComponent<CardBehavior>().isSpecial && this.cardInTable.GetComponent<CardBehavior>().corCarta == CardBehavior.CardColor.UNCOLOR
                    || card.GetComponent<CardBehavior>().isSpecial && card.GetComponent<CardBehavior>().corCarta == CardBehavior.CardColor.UNCOLOR)
                {
                    Debug.Log("Valor:" +  card.GetComponent<CardBehavior>().valorCarta + "Cor:" + card.GetComponent<CardBehavior>().corCarta);
                    this.state = Turn.PLAYER_1;
                    card.GetComponent<CardBehavior>().canInteract = true;
                    card.transform.position = cardInTable.transform.position;
                    oponentHand.Remove(card);
                    hasPlayed = true;
                    Debug.Log("Eu tenho");
                    break;
                }            
            }
            if (!hasPlayed)
            {
                GameObject cardBuy = cardInDeck[cardInDeck.Count - 1];
                cardInDeck.Remove(cardBuy);
                cardBuy.transform.position = players[1].transform.position;
                oponentHand.Add(cardBuy);
                cardBuy.GetComponent<CardBehavior>().isPlayer = false;
                cardBuy.GetComponent<CardBehavior>().isNotPlayer = true;
                cardBuy.GetComponent<CardBehavior>().isDeck = false;
                if (this.cardInTable.GetComponent<CardBehavior>().valorCarta == cardBuy.GetComponent<CardBehavior>().valorCarta
                   || this.cardInTable.GetComponent<CardBehavior>().corCarta == cardBuy.GetComponent<CardBehavior>().corCarta
                   || this.cardInTable.GetComponent<CardBehavior>().isSpecial && this.cardInTable.GetComponent<CardBehavior>().corCarta == CardBehavior.CardColor.UNCOLOR
                   || cardBuy.GetComponent<CardBehavior>().isSpecial && cardBuy.GetComponent<CardBehavior>().corCarta == CardBehavior.CardColor.UNCOLOR)
                {
                    Debug.Log("Valor:" + cardBuy.GetComponent<CardBehavior>().valorCarta + "Cor:" + cardBuy.GetComponent<CardBehavior>().corCarta);
                    this.state = Turn.PLAYER_1;
                    cardBuy.GetComponent<CardBehavior>().canInteract = true;
                    cardBuy.transform.position = cardInTable.transform.position;
                    cardBuy.transform.rotation = cardInTable.transform.rotation;
                    oponentHand.Remove(cardBuy);
                    Debug.Log("Eu comprei e tenho");
                }
                else
                {
                    Debug.Log("Não tenho");
                    opponentTurn = false;
                    this.state = Turn.PLAYER_1;
                }    
            }
            Debug.Log("Finalizei minha jogada");
            hasPlayed = false;
            opponentTurn = false;
            this.state = Turn.PLAYER_1;
            idTurn = 1;
            PutInTable();
        }
    }

    public IEnumerator StateMachine()
    {
        Debug.Log("Entrou no State");
        opponentTurn = true;
        yield return new WaitForSeconds(3);
        this.state = Turn.PLAYER_2;
        Debug.Log("Saiu do State");
    }

    public IEnumerator BackToMenu()
    {
        Debug.Log("Acabou");
        yield return new WaitForSeconds(5);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
