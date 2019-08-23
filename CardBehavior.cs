using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class CardBehavior : MonoBehaviour {

    public enum CardColor
    {
        LARANJA,
        VERMELHO,
        MARROM,
        AMARELO,
        UNCOLOR
    }

    [Header("Sprite")]
    public Sprite mySprite;
    private Dictionary<string, Sprite> dictSprites = new Dictionary<string, Sprite>(); //Dicionário de cartas na mão
    private Dictionary<string, Sprite> tableSprites = new Dictionary<string, Sprite>(); //Dicionário de cartas da mesa(Sem cor, só com nome)
    public Vector3 scaleUp, scaleDown;

    [Header("Controladores")]
    public float random;
    public bool isPlayer, isDeck, isNotPlayer, isTable;
    public List<int> ints;
    public bool canInteract; //Controla se a carta já pode interagir com a carta que está na mesa
    public bool hasReleased, hasReleasedInTrigger; //Ativa quando o player solta o botão do mouse, para controle de algumas funções.
    public Vector2 positionToChange; //A variável que controlará a posição do player durante o game.
    public float speedToTranslate; //A variável que controla o speed que a carta irá se mover de volta para sua posição inicial.

    GameManager gameManager;
    Quaternion quart; //Pequena Rotação que a carta terá quando for colocada na mesa

    [Header("Status")]
    public CardColor corCarta;
    public float valorCarta;
    public bool isSpecial;

    [Header("Variáveis Movimento")]
    public Vector2 startPosition;

    [Header("Componentes de Áudio")]
    public AudioSource audioSource;
    public AudioClip audioClip;

    public Dictionary<string, Sprite> DictSprites
    {
        get
        {
            return  dictSprites;
        }
        set
        {
            dictSprites = value;
        }
    }
    public Dictionary<string, Sprite> TableSprites
    {
        get
        {
            return tableSprites;
        }
        set
        {
            tableSprites = value;
        }
    }

    [Header("Visuais")]
    public ParticleSystem CardPut, CardBuy;

    void Start ()
    {
        speedToTranslate = 5;
        quart = new Quaternion(this.gameObject.transform.rotation.x, this.gameObject.transform.rotation.y, this.transform.rotation.z + Random.Range(-0.15f, 0.15f), this.gameObject.transform.rotation.w);
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 1.0f;
        this.scaleDown = this.gameObject.transform.localScale;
        scaleUp = new Vector3(0.6f, 0.6f, 0.6f);
        gameManager = GameObject.FindObjectOfType<GameManager>();
        this.gameObject.transform.parent = GameObject.Find("Cards").transform;
        GetRandomNumber();
        DefineCard();
        CardsStats();
        this.startPosition = this.gameObject.transform.position;
        CardPut = GetComponentInChildren<ParticleSystem>();
    }
    private void Update()
    {
        if(isTable && gameManager.turnTheCard)
        {
            GetComponent<SpriteRenderer>().sprite = tableSprites["cards2_" + random];
            this.gameObject.transform.rotation = quart;
            this.gameObject.tag = "Table";
            this.transform.position = gameManager.firstInTablePosition.transform.position;
        }
        else if (isTable && gameManager.turnTheCard == false)
            GetComponent<SpriteRenderer>().sprite = tableSprites["cards_" + random];
    }

    private void OnMouseEnter()
    {
                if (isPlayer || isTable)
            {
                this.gameObject.transform.localScale = scaleUp;
                this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
                audioClip = Resources.Load<AudioClip>("chipLay");
                audioSource.clip = audioClip;
                audioSource.Play();
            }
    }
    private void OnMouseExit()
    {
        this.gameObject.transform.localScale = scaleDown;
        this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;
       
    }
    void OnMouseDown()
    {
        if (gameManager.state != GameManager.Turn.PLAYER_1 || gameManager.opponentTurn)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
        if (isDeck)
            {
            GetComponent<SpriteRenderer>().sprite = dictSprites["cards_" + random];
            CardPut.Play();
            this.startPosition = gameManager.players[0].transform.position;
            this.gameObject.transform.position = gameManager.players[0].gameObject.transform.position;
            audioClip = Resources.Load<AudioClip>("cardSlide");
            audioSource.clip = audioClip;
            audioSource.Play();
            gameManager.cardInDeck.Remove(this.gameObject);
            gameManager.cardsInTable += 1;
            gameManager.playerHand.Add(this.gameObject);
            if (this.gameManager.cardInTable.GetComponent<CardBehavior>().valorCarta == this.valorCarta
               || this.gameManager.cardInTable.GetComponent<CardBehavior>().corCarta == this.corCarta
               || this.gameManager.cardInTable.GetComponent<CardBehavior>().isSpecial && this.corCarta == CardColor.UNCOLOR
               || this.isSpecial && this.corCarta == CardColor.UNCOLOR) 
            {
                this.gameObject.transform.position = gameManager.cardInTable.gameObject.transform.position;
                canInteract = true;
                this.isPlayer = true;
                this.isDeck = false;
            }
            else
            {
                gameManager.StartCoroutine(gameManager.StateMachine());
                gameManager.idTurn = 2;
                gameManager.PutInTable();
                this.isPlayer = true;
                this.isDeck = false;
            }  
        }
            if (isPlayer)
                canInteract = true;
                audioClip = Resources.Load<AudioClip>("cardSlide");
                audioSource.clip = audioClip;
                audioSource.Play();
				//else if(isTable)
				//    GetComponent<SpriteRenderer>().sprite = dictSprites["cards_" + random];
    }
    void OnMouseDrag()
    {
        if (gameManager.state != GameManager.Turn.PLAYER_1 || gameManager.opponentTurn)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
        if (isPlayer)
        {
            float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
            Vector3 pos_move = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
            transform.position = new Vector3(pos_move.x, pos_move.y, pos_move.z);
        }
    }

    private void OnMouseUp()
    {
        if (gameManager.state != GameManager.Turn.PLAYER_1 || gameManager.opponentTurn)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
        if (isPlayer)
        {
            if (hasReleasedInTrigger)
                hasReleased = true;
            //this.transform.position = startPosition;
            audioClip = Resources.Load<AudioClip>("cardPlace");
            audioSource.clip = audioClip;
            audioSource.Play();        
        }
    }
    public void TransformToStartPosition()
    {
        float speed = speedToTranslate * Time.deltaTime;
        this.gameObject.transform.position = Vector3.MoveTowards(transform.position, startPosition, speed);
    }

    public void DefineCard()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("cards");
        Sprite[] spritesTable = Resources.LoadAll<Sprite>("cards2");

        foreach (Sprite sprite in sprites)
        {
            dictSprites.Add(sprite.name, sprite);
        }
        foreach (Sprite sprite in spritesTable)
        {
            tableSprites.Add(sprite.name, sprite);
        }

        if (isPlayer)
        {          
            GetComponent<SpriteRenderer>().sprite = dictSprites["cards_" + random];
            this.gameObject.tag = "Player";
            gameManager.playerCards.Add(this.gameObject);
            gameManager.playerHand.Add(this.gameObject);
        }
        else if (isNotPlayer)
        {
            gameManager.oponentHand.Add(this.gameObject);
            GetComponent<SpriteRenderer>().sprite = dictSprites["cards_" + 41];
        }
        else
            GetComponent<SpriteRenderer>().sprite = dictSprites["cards_" + 41];
    }
    public float GetRandomNumber()
    {
        do random = Random.Range(0, 40);
        while (gameManager.cardUsed.Contains(random));
        gameManager.cardUsed.Add(random);
        return random;
    }
    public void CardsStats()
    {
        if(random >= 0 && random <= 8)
        {
            this.corCarta = CardColor.LARANJA;
            valorCarta = random +2;
            if(valorCarta == 10)
            {
                valorCarta = 0;
            }
        }
        else if (random >= 9 && random <= 17)
        {
            this.corCarta = CardColor.VERMELHO;
            valorCarta = random - 7;
            if (valorCarta == 10)
            {
                valorCarta = 0;
            }
        }
        else if (random >= 18 && random <= 26)
        {
            this.corCarta = CardColor.MARROM;
            valorCarta = random - 16;
            if (valorCarta == 10)
            {
                valorCarta = 0;
            }
        }
        else if (random >= 27 && random <= 35)
        {
            this.corCarta = CardColor.AMARELO;
            valorCarta = random - 25;
            if (valorCarta == 10)
            {
                valorCarta = 0;
            }
        }
        else
        {
            isSpecial = true;
            this.corCarta = CardColor.UNCOLOR;
        }
    }

    //public void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (collision.gameObject.tag == "Table" && canInteract)
    //    {
    //        if (collision.gameObject.GetComponent<CardBehavior>().valorCarta == this.valorCarta || collision.gameObject.GetComponent<CardBehavior>().corCarta == this.corCarta
    //            || this.isSpecial && corCarta == CardColor.UNCOLOR)
    //        {
    //            TransformToStartPosition();
    //        }

    //        hasReleasedInTrigger = true;    
    //    }
    //}

    //public void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.gameObject.tag == "Table" && canInteract)
    //    {
    //        hasReleasedInTrigger = false;
    //    }
    //}

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Table" && canInteract)
        {
                if (collision.gameObject.GetComponent<CardBehavior>().valorCarta == this.valorCarta 
                || collision.gameObject.GetComponent<CardBehavior>().corCarta == this.corCarta
                || this.isSpecial && corCarta == CardColor.UNCOLOR 
                || collision.gameObject.GetComponent<CardBehavior>().isSpecial 
                && collision.gameObject.GetComponent<CardBehavior>().corCarta == CardColor.UNCOLOR)
                {
                if (isPlayer)
                {
                    this.gameObject.tag = "Table";
                    this.isPlayer = false;
                    this.isTable = true;
                    this.canInteract = false;
                    //this.startPosition = collision.gameObject.transform.position;
                    gameManager.cardUsed.Remove(collision.gameObject.GetComponent<CardBehavior>().random);
                    gameManager.cardsInTable -= 1;
                    Destroy(collision.gameObject);
                    Debug.Log("Match");
                    hasReleased = false;
                    Debug.Log("EnemyTurn");
                    this.gameManager.cardInTable = this.gameObject;
                    gameManager.StartCoroutine(gameManager.StateMachine());
                    gameManager.idTurn = 2;
                    gameManager.PutInTable();
                    gameManager.playerHand.Remove(this.gameObject);
                    CardPut.Play();
                }
                else if(isNotPlayer)
                {
                    audioClip = Resources.Load<AudioClip>("cardPlace");
                    audioSource.clip = audioClip;
                    audioSource.Play();
                    this.gameObject.tag = "Table";
                    this.isDeck = false;
                    this.isNotPlayer = false;
                    this.canInteract = false;
                    this.isPlayer = false;
                    this.isTable = true;
                    //this.startPosition = collision.gameObject.transform.position;
                    gameManager.cardUsed.Remove(collision.gameObject.GetComponent<CardBehavior>().random);
                    gameManager.cardsInTable -= 1;
                    Destroy(collision.gameObject);
                    this.gameManager.cardInTable = this.gameObject;
                    this.gameObject.transform.rotation = collision.transform.rotation;
                    CardPut.Play();
                    //collision.gameObject.SetActive(false);
                    //collision.gameObject.transform.Translate(new Vector2(20, 0));
                    Debug.Log("Match");
                    Debug.Log("PlayerTurn");
                    gameManager.state = GameManager.Turn.PLAYER_1;

                }
                    //for(int i = 0; i <= gameManager.playerCards.Count; i++)
                    //{
                    //    if(gameManager.playerCards[i] > )
                    //}
                }

                else
                {
                    gameManager.invalidMatch = true;
                    gameManager.MatchsControl();
                    Debug.Log("Unmatch");
                    hasReleased = false;
                }        
        }
    }
}
