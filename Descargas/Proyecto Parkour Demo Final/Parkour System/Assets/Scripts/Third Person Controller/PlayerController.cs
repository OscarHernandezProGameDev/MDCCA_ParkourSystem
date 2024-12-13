using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnvironmentScanner;

public class PlayerController : MonoBehaviour
{
    // Referencia al recolector de inputs
    [SerializeField] private GatherInput gatherInput;

    // Referencias a la velocidad de movimiento y de rotación
    [SerializeField] private float moveSpeed = 5f, rotationSpeed = 500f;

    // *Añadidos* => Referencias para las nuevas acciones de esprintar y saltar
    [SerializeField] private float extraSpeed = 1.5f, actualSpeed, jumpPower = 8f;
    private bool isJumping;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPositionLeft;
    [SerializeField] private Transform groundCheckPositionRight;

    [SerializeField] private float groundCheckRadious = 0.2f;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;

    // Referencias a componentes
    private CameraController cameraController;
    private CharacterController characterController;
    private ParkourController parkourController;
    private EnvironmentScanner environmentScanner;
    private Animator animator;

    // Variables de movimiento
    private Vector3 moveInput;          // dirección
    private Quaternion targetRotation; // rotación
    private float ySpeed;             // velocidad en el eje Y usado para la gravedad
   
    // ¿El jugador tiene el control? por defecto si
    private bool hasControl = true;

    // ¿Está el jugador haciendo una acción?
    public bool InAction {  get; private set; }

    // ¿Está el jugador en estado agarrado?
    public bool IsHanging { get; set; }

    // Dirección de movimiento principal, lo usaremos para mover al personaje (o saber hacia donde quiere ir)
    private Vector3 desiredMoveDir;

    // Dirección de movimiento adicional, lo usaremos para controlar la cámara
    private Vector3 moveDir;

    // Velocidad del personaje , es la variable final que le pasamos a cc.Move para movernos y aplicar la gravedad
    private Vector3 velocity;

    // ¿Está el player al borde de un saliente?
    public bool IsOnLedge { get; set; }
    
    // Estructura que contendrá la data sobre los salientes
    public LedgeData LedgeData { get; set; }

    // *Añadidos* => Tiempo umbral para ejecutar un Hard Landing (ahora al caer, según la altura, ejecuta un aterrizaje u otro)
    [SerializeField] private float fallThreshold = 0.5f;            

    // *Añadidos* => Tiempo total de caída
    private float fallTime; 



    private void Awake()
    {
        // Asignación de componentes
        cameraController = Camera.main.GetComponent<CameraController>();
        characterController = GetComponent<CharacterController>();
        parkourController = GetComponent<ParkourController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        Application.targetFrameRate = 60; //Objetivo de renderizado: 60 fps
    }

    void Update()
    {
        // Tomamos la dirección ya suavizada en un vector 2
        Vector2 direction = gatherInput.smoothedDirection;

        // Convertimos la dirección suavizada en un Vector3 para el movimiento 3D
        moveInput = new Vector3(direction.x, 0, direction.y);
  
        // "Cantidad" de movimiento absoluto(sin números negativos), capado entre 0 y 1
        float moveAmount = Mathf.Clamp01(Mathf.Abs(direction.x) + Mathf.Abs(direction.y));

        // A la dirección de movimiento le añadimos la rotación de la cámara, de este modo podremos dirigirnos hacia donde enfoque la cámara
        desiredMoveDir = cameraController.GetYRotation * moveInput;

        // Añadimos a moveDir la dirección calculada, luego la usaremos para la rotación
        moveDir = desiredMoveDir;

        // Si el jugador no tiene el control o se está agarrando... devolvemos método
        if (!hasControl || IsHanging) 
            return;

        // *Añadido* => Siguiente línea comentada tras la implementación del salto, si no lo hacemos, el salto deja de seguir su rumbo al soltar el input
        //velocity = Vector3.zero;                                       

        //Le pasamos al animator el valor de la función IsGrounded, para que siempre sepa cuando está tocando suelo
        animator.SetBool("isGrounded", IsGrounded());
      
        // *Añadido* => ahora toda la lógica de ISGrounded() sucede dentro de este método, código viejo comentado abajo => Contiene nueva acción de salto
        ApplyGravity();

        #region Código IsGrounded Original
        /*   if (IsGrounded())
           {
              // ySpeed = -1;

               velocity = desiredMoveDir * actualSpeed;

               IsOnLedge = environmentScanner.ObstacleLedgeCheck(desiredMoveDir.normalized, out LedgeData ledgeData); 

               if(IsOnLedge) 
               {
                   LedgeData = ledgeData;
                   LedgeMovement();
               }

               fallTime = 0;

               float maxSpeed = moveSpeed + extraSpeed;
               animator.SetFloat("moveAmount", velocity.magnitude/ maxSpeed, 0.1f, Time.deltaTime);
           }
           else
           {

               velocity = transform.forward * moveSpeed / 2;

               ySpeed += Physics.gravity.y * Time.deltaTime;

               // Incrementar el tiempo de caída
               fallTime += Time.deltaTime;

               if (fallTime > fallThreshold)
               {
                   animator.SetBool("HardLanding", true);
               }
               else
               {
                   animator.SetBool("HardLanding", false);
               }
           }
        */
        #endregion

        // *Añadido* => Actualiza la velocidad según si el jugador está corriendo o no (Nueva acción de Sprint)
        if (gatherInput.tryToRun)
            actualSpeed = moveSpeed + extraSpeed;
        else
            actualSpeed = moveSpeed;

        // *Añadido* => Si el jugador no está en frente de un obstáculo, no está en estado agarrado, ni en un saliente, ni realizando otra acción y pulsa saltar...
        if (gatherInput.tryToJump && !InAction && !isJumping && !IsOnLedge && !IsHanging && !environmentScanner.InFrontOfObstacle)
        {
            ySpeed += jumpPower;                    // ...Añadimos al eje Y la potencia del salto... 
            StartCoroutine(DoAction("NormalJump")); // ...Acionamos la animación de salto normal...
            isJumping = true;                       //... Confirmamos que estamos saltando...
            gatherInput.tryToJump = false;         // ... y reiniciamos el valor de tryToJump para obligar al jugador a soltar la tecla
        }

        // *Añadido* => Siguiente Variable Comentada. Ahora le pasamos ySpeed a velocity dentro del método ApplyGravity.
        // velocity.y = ySpeed;

        // Movemos al personaje a través del Move del cc, pasándole la velocidad por el tiempo
        characterController.Move(velocity * Time.deltaTime);

        // Si hay movimiento, rota el personaje hacia la dirección de movimiento
        if (moveAmount > 0f && moveDir.sqrMagnitude > 0.05f)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }
        else
        {
            velocity = Vector3.zero;
        }

        // Rota suavemente el personaje hacia el targetRotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Método para caminar por el borde de los salientes
    private void LedgeMovement()
    {
        // Calcula el ángulo firmado entre la normal de la superficie donde el jugador está (LedgeData.surfaceHit.normal) y la dirección de movimiento deseada (desiredMoveDir). 
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDir, Vector3.up);           // El ángulo se mide alrededor del eje vertical (Vector3.up).

        // Obtenemos el valor absoluto del ángulo firmado para evaluar la inclinación con respecto a la cornisa.
        float angle = Mathf.Abs(signedAngle);

        // Verificamos si el ángulo entre la dirección deseada y la parte frontal del jugador es igual o mayor que nuestro umbral (80)
        if (Vector3.Angle(desiredMoveDir,transform.forward) >= 80)
        {
            // Detenemos el movimiento dado que estaremos cerca de encarar un saliente
            velocity = Vector3.zero;
            return;
        }

        // Si ya estamos bastante encarados (<50), detenemos el movimiento, y decidimos si capar también la rotación (leer comentario + abajo).
        if(angle < 50) 
        {
             velocity = Vector3.zero;
             // Comenta la siguiente línea para permitir que el player rote al llegar a una cornisa =>
             moveDir = Vector3.zero;                          // Rotar previene que si el player hace "el tonto" en una esquina, nos caigamos eventualmente
                                                              // Como contra, el "feeling" de rotar sin el movimiento, puede ser peor que sin la rotación
        }
        else if(angle < 90)
        {
            // Si el ángulo es menor que  90, limitamos velocity a solo la dirección horizontal del saliente en el que estemos para permitir el movimiento laterial por el borde de los salientes

            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * dir;
            moveDir = dir;
        }
    }

    // Corrutina para realizar acciones de Parkour y de Escalada (Climb)
    public IEnumerator DoAction(string animName, MatchTargetParams matchParams = null, Quaternion targetRotation = new Quaternion(),
                                                                         bool rotate = false, float postDelay = 0f, bool mirror = false)
    {
        // Confirmamos que el jugador está realizando una acción
        InAction = true;

        // Le pasamos al animator el valor de mirror según le hayamos especificado
        animator.SetBool("mirrorAction", mirror);

        // Reproducimos las animaciones que le pasemos usamos CrossFade para interpolar entre ellas (Transición entre anims)
        animator.CrossFadeInFixedTime(animName, 0.2f);

        // Esperamos un frame
        yield return null;

        // Tomamos información sobre la animación actual y la guardamos en animState
        var animState = animator.GetNextAnimatorStateInfo(0);

        // Si el nombre proporcionado no existe, avisamos con un LogError
        if (!animState.IsName(animName))
        {
            Debug.LogError("El nombre de la animación proporcionado no existe. Revisa los nombres en el Animator. ");
        }

        // Tomamos el valor del startTime del Target Matching para que el jugador rote justo cuando empieza el target Matching, si es nulo, le pasamos 0.
        float rotateStartTime = (matchParams != null)? matchParams.startTime : 0f;

        // Abrimos temporizador a 0 para contar la animación
        float timer = 0f;

        // Mientras el temporizador sea menor que la longitud de la animación..
        while (timer < animState.length)
        {
            // ...Empezamos a contar el tiempo
            timer += Time.deltaTime;

            // Normalizamos el tiempo del temporizador (rotateStartTime está normalizado, así que normalizamos ambos)
            float normalizedTime = timer / animState.length;

            // Si hemos marcado que debe rotar y el tiempo normalizado es mayor que el tiempo definido para la rotación, ¡Forzamos la rotación del jugador hacia el obstáculo!
            if (rotate && normalizedTime > rotateStartTime)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                                                                rotationSpeed * Time.deltaTime);

            // Si matchParams tiene información, realizamos Target Matching con esa data.
            if (matchParams != null)
                MatchTarget(matchParams);

            // Si la animación ha empezado a transicionar a la siguiente y el temporizador  es mayor que 0.5f (medida de seguridad para no romper el bucle antes de tiempo)
            if (animator.IsInTransition(0) && timer > 0.5f)
                break; //Rompemos el bucle para que la gravedad pueda volver a aplicarse (Esto soluciona problemas de offsets en Y en obstáculos horizontales como el vaultFence)

            yield return null;//Esperamos un frame
        }

        // Si postDelay es mayor que 0, esperaremos ese tiempo antes de continuar
        yield return new WaitForSeconds(postDelay);

        // La acción ha finalizado
        InAction = false;

        // *Añadido* => Si AbleToAutoJump es falso, lo marcamos como verdadero ya que ya no estamos realizando una acción, por lo tanto, el autoJump volvería a estar disponible.
        if(!parkourController.AbleToAutoJump)
            parkourController.AbleToAutoJump = true;

    }

    // Método para ejecutar el Target Matching del animator... * antes en ParkourController *
    private void MatchTarget(MatchTargetParams mtp)
    {
        if (animator.isMatchingTarget || animator.IsInTransition(0)) return;

        animator.MatchTarget(mtp.pos, transform.rotation, mtp.bodyPart,
            new MatchTargetWeightMask(mtp.posWeight, 0), mtp.startTime, mtp.targetTime);
    }

    // *Añadido* => Nuevo método para la aplicación de  gravedad, incluye nueva acción de salto y nueva acción de aterrizar
    private void ApplyGravity()
    {
        if (IsGrounded() && ySpeed < 0f) // Si estamos tocando el suelo e ySpeed es menor que 0
        {
            // Aplicamos una fuerza para que el jugador se pegue bien al suelo
            ySpeed = -1f;

            // Si estaba saltando y estamos en IsGrounded(true), es que acaba de aterrizar de una caída (has landed)
            if (isJumping) { isJumping = false; } 

            // Multiplicamos nuestra dirección correjida por la cámara por speed
            velocity = desiredMoveDir * actualSpeed;

            // Comprobamos si estamos en un saliente (normalizamos desiredMoveDir para evitar que el "rebote" de smoothedDirection empuje al jugador hacia fuera al soltar la tecla)
            IsOnLedge = environmentScanner.ObstacleLedgeCheck(desiredMoveDir.normalized, out LedgeData ledgeData);

           // Si estamos en un saliente, tomamos la data y ejecutamos el movimiento en salientes
            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }

            // Como estamos en el suelo, reiniciamos el contador de la caída 
            if(fallTime !=0) { fallTime = 0f; }

            // *Añadido* => Nueva variable de maxSpeed que englobe nuestra velocidad con el posible extra para pasárselo al moveAmount del animator.
            float maxSpeed = moveSpeed + extraSpeed;

            // Configuramos moveAmount en el animator a la vez que lo normalizamos con magnitude/ velocidad. (antes moveSpeed, ahora maxSpeed)
            animator.SetFloat("moveAmount", velocity.magnitude / maxSpeed, 0.1f, Time.deltaTime);
        }
        else
        {
            // *Añadido* => Si no estamos realizando un salto simple es que estamos cayendo, si caemos, avanzamos hacia donde nos movíamos a la mitad de velocidad
            if (!isJumping) velocity = transform.forward * moveSpeed / 2;

            // Aplicamos la gravedad al valor ySpeed
            ySpeed += Physics.gravity.y * Time.deltaTime;

            // Según la duración de la caída, ejecutamos una animación u otra en el método  ChooseLandingAnim();
            ChooseLandingAnim();
        }

        // Pasamos ySpeed al eje Y de nuestro Vector 3 velocity para aplicar gravedad (y ahora saltos)
        velocity.y = ySpeed;
    }

    // Método para Elegir animación de aterrizaje correspondiente al caer.
    private void ChooseLandingAnim()
    {
        // Mientras caemos, el temporizador empieza a contar
        fallTime += Time.deltaTime;

        // Si el tiempo es mayor que el umbral definido en fallThreshold, ejecutamos un aterrizado "duro" (HardLanding) cambiando la booleana en el animator
        if (fallTime > fallThreshold)
        {
            animator.SetBool("HardLanding", true);
        }
        else // Si el tiempo es menor que el umbral, ejecutamos un aterrizado "normal" al dejar en falso la booleana "HardLanding" en el animator
        {
            animator.SetBool("HardLanding", false);
        }
    }

    // Método para darle o quitarle el control a player
    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!hasControl) // Si no tien el control, forzamos el moveAmount del animator a 0  y reiniciamos targetRotation para que no haya problemas de rotación
        {
            animator.SetFloat("moveAmount", 0);
            targetRotation = transform.rotation;
        }
    }

    // Método para activar/ desactivar el Collider del Character Controller, sin devolverle el control
    public void EnableCharacterController(bool enabled)
    {
       characterController.enabled = enabled;
    }

    // Método para reiniciar el target Rotation a la rotación actual del jugador (Rotaba automáticamente en el aire al ejecutar Drop Action, así evitamos giros indeseados)
    public void ResetTargetRotation()
    {
        targetRotation = transform.rotation;
    }

    // Propiedad pública para obtener la variable sobre si el jugador tiene el control directo o no
    public bool HasControl
    {
        get => hasControl;
        set => hasControl = value;
    }

    // *Actualizado* => Antiguo GroundCheck. Desde la implementación del salto simple, duplicamos la esfera de los pies a 2 para un mayor control sobre si un pie ha aterrizado
    private bool IsGrounded()
    {
        bool isGrounded;

        //Esferas pegadas en los pies para comprobar si tocan suelo
        bool leftShoe = Physics.CheckSphere(groundCheckPositionLeft.transform.TransformPoint(groundCheckOffset), groundCheckRadious, groundLayer);
        bool rightShoe = Physics.CheckSphere(groundCheckPositionRight.transform.TransformPoint(groundCheckOffset), groundCheckRadious, groundLayer);
      
        //Si alguno de los 2 pies está tocando suelo, devolvemos true, de lo contrario, devolvemos falso.
        if (leftShoe || rightShoe)
            isGrounded = true;
        else
            isGrounded = false;

        return isGrounded;
    }

    // Dibujado de las esferas en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(groundCheckPositionLeft.transform.TransformPoint(groundCheckOffset), groundCheckRadious);
        Gizmos.DrawSphere(groundCheckPositionRight.transform.TransformPoint(groundCheckOffset), groundCheckRadious);
    }

    // Propiedad pública para exponer rotation Speed
    public float RotationSpeed => rotationSpeed;

}

// Clase para pasar la información referente al Target Matching
public class MatchTargetParams
{
    public Vector3 pos;           // Posición a encajar
    public AvatarTarget bodyPart; // Parte del cuerpo a encajar
    public Vector3 posWeight;     // Vector 3 con los pesos de los ejes a encajar
    public float startTime;       // Valor start time del target matching    
    public float targetTime;      // Valor target time del target matching
}