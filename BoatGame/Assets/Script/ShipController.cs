
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ShipController : MonoBehaviour
{
    [field: SerializeField] 
    private float Acceleration {get;set;} = 28f;

    [field: SerializeField] 
    private float MaxSpeed {get;set;} = 50f;

    [field: SerializeField] 
    private float TurnSpeed {get;set;} = 8f;

    [field: Header("Life Compoments")]
    private float _life = 100;
    
    public float Life
    {
        get => _life;
        set
        {
            if (_life != value) 
            {
                _life = value;
                LifeBar();   
            }
        }
    }

    [field: SerializeField]
    private Image LifeFill {get; set;}

    [field: SerializeField]
    private UiManager UiManager {get; set;}
    [field: SerializeField]
    private AudioSource AudioSource {get; set;}
    [field: SerializeField]
    private AudioSource GameOverSource {get; set;}

    [field: SerializeField]
    private AudioClip AudioClip {get; set;}

    [field: SerializeField]
    private AudioClip DamageClip {get; set;}


    private Rigidbody rb;

    private bool _canTakeDamage = true;

    [field: SerializeField]
    private CanvasGroup DEBUG{get; set;}

    [field: SerializeField]
    private Slider DEBUGACC{get; set;}
    [field: SerializeField]
    private TextMeshProUGUI DEBUGTEXTACC{get; set;}

    [field: SerializeField]
    private Slider DEBUGVELOCIDADE{get; set;}
    [field: SerializeField]
    private TextMeshProUGUI DEBUGTEXTVELOCIDADE{get; set;}
    
    [field: SerializeField]
    private Slider DEBUGROT{get; set;}
    [field: SerializeField]
    private TextMeshProUGUI DEBUGTEXTROT{get; set;}

    private bool _isDead = false;

    private void Awake()
    {
        _canTakeDamage = true;
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -1f, 0); 
        _isDead = false;
        DEBUGACC.value = 25f;
        DEBUGVELOCIDADE.value = 40;
        DEBUGROT.value = 9;
        DEBUG.alpha = 0;
    }

    void Start()
    {
        DEBUGACC.onValueChanged.AddListener(value =>
        {
            Acceleration = value;
            DEBUGTEXTACC.text = Acceleration.ToString();
        });
        DEBUGVELOCIDADE.onValueChanged.AddListener(value =>
        {
            MaxSpeed = value;
            DEBUGTEXTVELOCIDADE.text = MaxSpeed.ToString();
        });
        DEBUGROT.onValueChanged.AddListener(value =>
        {
            TurnSpeed = value;
            DEBUGTEXTROT.text = TurnSpeed.ToString();
        });

    }

    void Update()
    {
        if(Life <= 0 && !_isDead)
        {
            if(!GameOverSource.isPlaying)
                GameOverSource.PlayOneShot(AudioClip);
            UiManager.GameOver();
            _isDead = true;
        }

        if(Input.GetKeyDown(KeyCode.K))
            DEBUG.alpha = DEBUG.alpha == 1? 0 : 1;

          
    }

    private void FixedUpdate()
    {
        float forward = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        rb.AddForce(transform.forward * forward * Acceleration, ForceMode.Acceleration);

        float speedFactor = rb.linearVelocity.magnitude / MaxSpeed;
        rb.AddTorque(Vector3.up * turn * TurnSpeed * speedFactor, ForceMode.Acceleration);

        Vector3 flat = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (flat.magnitude > MaxSpeed)
        {
            flat = flat.normalized * MaxSpeed;
            rb.linearVelocity = new Vector3(flat.x, rb.linearVelocity.y, flat.z);
        }
    }



    void LifeBar()
    {
        LifeFill.rectTransform.DOAnchorMax(new Vector2((Life/100),1f), .2f);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Obstacle") && _canTakeDamage)
        {
            Life -= 20;
            AudioSource.PlayOneShot(DamageClip);
            Destroy(other.gameObject);
            StartCoroutine(Cooldown());
        }
        if(other.CompareTag("Terrain") && _canTakeDamage)
        {
            AudioSource.PlayOneShot(DamageClip);
            Life -= 20;
        }
        if(other.CompareTag("Winning"))
        {
            UiManager.Win();
        }
    } 

    IEnumerator Cooldown()
    {
        _canTakeDamage = false;
        yield return new WaitForSeconds(3f);
        _canTakeDamage = true;
    }
}
