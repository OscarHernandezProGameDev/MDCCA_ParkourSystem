using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Clase para gestionar la captura de inputs del jugador utilizando el sistema de Input System de Unity.
public class GatherInput : MonoBehaviour
{

    private PlayerInput playerInput;

    //Acciones
    private InputAction lookAction; // Acci�n para mirar (movimiento de c�mara)
    private InputAction moveAction; // Acci�n para moverse (direcci�n del jugador)
    private InputAction jumpAction; // Acci�n para saltar o accionar acciones de parkour/Climb system
    private InputAction dropAction; //Acci�n para descolgarnos del estado "Hanging"

    private InputAction runAction; // *A�adido* Acci�n para correr
    private InputAction pauseMenuAction; // *A�adido* Acci�n para abrir/cerrar el men� de pausa (se cre� para la Build)

    // Tiempo para suavizar la interpolaci�n del input de movimiento // M�s bajo = movimiento m�s pesado (curva m�s larga del 0 al 1 y viceversa)
    [SerializeField] private float smoothTime = 4f;                  // M�s alto = movimiento m�s r�pido (curva m�s corta del 0 al 1 y viceversa)

    // Vector de entrada para la acci�n de mirar
    public Vector2 lookInput;

    // Direcci�n suavizada del movimiento del jugador
    public Vector2 smoothedDirection;

    // Direcci�n de movimiento cruda
    private Vector2 _direction;

    // Propiedad p�blica para obtener la direcci�n
    public Vector2 Direction { get => _direction; }

    // Boleanas para confirmar si pulsamos los inputs correspondientes de estas acciones
    public bool tryToJump;
    public bool tryToDrop;
    public bool tryToRun;

    //booleana que confirma si estamos usando gamepad
    public bool usingGamepad;

    // Referencia al men� de pausa en la UI
    [SerializeField] private GameObject pauseCanvas;

    private void Awake()
    {
        // Obtenemos componentes y acciones
        playerInput = GetComponent<PlayerInput>();
        
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dropAction = playerInput.actions["Drop"];

        runAction = playerInput.actions["FastRun"];
        pauseMenuAction = playerInput.actions["PauseMenu"];
    }

    private void OnEnable()
    {
        // Suscribimos acciones a fases y m�todos
        lookAction.performed += ReadLookInput;
        lookAction.canceled += OnLookCanceled;
        jumpAction.performed += Jump;
        jumpAction.canceled += OnJumpCanceled;
        dropAction.performed += Drop;
        dropAction.canceled += OnDropCanceled;

        runAction.performed += Run;
        runAction.canceled += OnRunCanceled;

        pauseMenuAction.performed += PauseMenuControl;
    }


    private void Update()
    {
        // Leemos la direcci�n del movimiento actual
        _direction = moveAction.ReadValue<Vector2>();

        // Suavizamos el movimiento del jugador hacia la direcci�n objetivo
        smoothedDirection = new Vector2(
            Mathf.MoveTowards(smoothedDirection.x, _direction.x, smoothTime * Time.deltaTime),
            Mathf.MoveTowards(smoothedDirection.y, _direction.y, smoothTime * Time.deltaTime)
            );
    }

    // Leemos el input de la c�mara y asociamos usingGamepad seg�n el dispositivo que usemos
    private void ReadLookInput(InputAction.CallbackContext context)
    { 
        lookInput = context.ReadValue<Vector2>();
        usingGamepad = context.control.device is Gamepad;
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
       lookInput = Vector2.zero; // Reiniciamos el input de mirar cuando se "cancela", de lo contrario, no podr�amos detener el movimiento
    }

    // M�todos para asociar las booleanas a sus estados pulsado/soltado =>
    private void Jump(InputAction.CallbackContext context)
    {
        tryToJump = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        tryToJump = false;
    }

    private void Drop(InputAction.CallbackContext context) 
    { 
        tryToDrop = true; 
    }

    private void OnDropCanceled(InputAction.CallbackContext context)
    {
        tryToDrop = false;
    }

    private void Run(InputAction.CallbackContext context)
    {
        tryToRun = true;
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        tryToRun = false;
    }

    // *A�adido para build*
    // Alternamos el estado del men� de pausa y la visibilidad del cursor
    private void PauseMenuControl(InputAction.CallbackContext context)
    {
        if (pauseCanvas.activeSelf)
        {
            Cursor.visible = false;
            pauseCanvas.SetActive(false);
        }
           
        else
        {
            Cursor.visible = true;
            pauseCanvas.SetActive(true);
        }
    }

    private void OnDisable()
    {
        // Desuscribimos acciones a fases y m�todos
        lookAction.performed -= ReadLookInput;
        lookAction.canceled -= OnLookCanceled;
        jumpAction.performed -= Jump;
        jumpAction.canceled -= OnJumpCanceled;
        dropAction.performed -= Drop;
        dropAction.canceled -= OnDropCanceled;

        runAction.performed -= Run;
        runAction.canceled -= OnRunCanceled;

        pauseMenuAction.performed -= PauseMenuControl;
    }

}
