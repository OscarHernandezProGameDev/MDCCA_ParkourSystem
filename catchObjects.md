Buenos días. Tengo una duda de como hacer coger items y otra preguntar. Ayer aunque estuve en el directo no estaba en casa, con el móvil y haciendo recados por lo que no pude preguntar. En la animación es una simple mecánica que hice para coger items. Lo que hacia cuando el collider del item se colisiona con el player, se aplica al player la animación que veis. En esta animación hay puesto 2 trigger para que llamen a dos métodos para que en el 2 desaparezca el item y se active en el rig del personaje la espada. 
@Daniel Roman
 o 
@Mariano
 hay alguna forma mas profesional de hacerlo y que quede bien. ¿En las mecánicas que veremos en el curso se puede aplicar a este caso?
La otra pregunta era en el bootcamp estamos usando un script para controlar la cámara en vez de usar cinematic. 
@Daniel Roman
 o 
@Mariano
 desde una vista profesional: ¿cuando se usa el cinematic?.

Hola Oscar! vamos por partes.
Respecto a la cámara, supongo que te refieres a cinemachine, no?
Cinemachine tiene toda una serie de configuraciones/funciones predeterminadas que podemos usar para múltiples situaciones, se puede usar para un controlador 3D como el nuestro, pero luego tiene muchas funciones dedicadas o más enfocadas a cinemáticas ( u otro tipo de vistas).
Nosotros hemos optado por crear la cámara desde 0, lo cual es un básico que creímos que sería más útil de forma general, y que estaría en línea con crear el controlador 3D desde 0 y por nosotros mismos.
No creo que se pueda decir que ninguna de las 2 opciones sea más profesional que la otra, ya que dependerá de para que la necesitamos (la cámara).
Ambas formas permiten que la cámara haga su función de forma profesional.
Al crearla nosotros, podemos implementar el comportamiento exacto que queramos sin que sobre nada y pudiendo extenderse hasta donde necesitemos.
Por otro lado, cinemachine tiene muchísimas funciones, si las necesitas, puedes ahorrarte mucho tiempo al no tener que crearlas/codificarlas por ti mismo.
En conclusión, hay que evaluar y valorar para usar una opción u otra según la necesidad y luego también según las preferencias, hay gente que usa siempre cinemachine y otra gente que prefiere crear siempre su propio controlador.
Mi consejo que controles ambas opciones si o si. Y si tu juego va a tener cinemáticas, entonces si que me iría directo a cinemachine, ya que funciones como el track Dolly son tremendamente útiles para ello.
__________________________________________________________________________________________
Luego, respecto a recoger items y a tu pregunta de si podemos usar lo aprendido en el bootcamp para recoger objetos, hablamos de funciones muy diferentes, las mecánicas que aprendemos en el bootcamp están relacionadas con la movilidad y el sorteo de obstáculos.
Quizás de pasada, puedan tener de relación el hecho de ejecutar acciones => en el bootcamp haremos acciones de parkour, y en lo que buscas, se realizarían acciones de recogida de objetos, podría servirte la estructura que haremos para realizar tales acciones, pero ya.
La forma profesional más común para recoger objetos suele ser mediante eventos y/o delegados. Suele ir de la mano de tener previamente un sistema de inventario aunque dependiendo del juego quizás no sea el caso.
Luego, normalmente también se recomendaría alguna interacción por parte del jugador, en lugar de recogerlo directamente, que el jugador deba presionar alguna tecla para hacer la acción de recogerlo.
Sería algo parecido a:
¿El jugador está encima de un objeto "recogible" y ha pulsado la tecla de recoger?
-Ejecuta la animación de recoger;
-Evento de recoger objeto (que objeto es, que cantidad, a donde debe ir y otros implicados como activarlo en el rig, etc);
Luego, el hecho de que el jugador levite cuando recoge el objeto debe ser cuestión de la configuración de la animación o de la animación en si.
Habría que ver si el fallo es de la misma animación o no.
Puedes probar de ir a la pestaña Animation del modelo, busca la opción Root Transform Position (Y) y prueba a poner el Baked In To Pose y a cambiar el Based Upon en Original o en Feet.
Aunque quizas no quieras usar Apply Root Motion, no lo sé, depende de lo que quieras.

ahh si, para hacer coincidir la mano con los objetos, usar Ik's es una opción muy buena
12:34
enfocando a esa caracterísitca de hacerlos coincidir, no estoy seguro de si el target matching podría funcionarnos para eso, es posible que si
12:34
nunca lo intenté para estos casos
12:35
En todo caso, una vez lleguemos a esa parte, podrás intentarlo con bastante facilidad



12:35
lo miramos juntos si hace falta

Pues empezaría por la doc de Unity,
Sobre el target matching, aquí : https://docs.unity3d.com/Manual/TargetMatching.html
y sobre el Ik, aquí: https://docs.unity3d.com/Manual/InverseKinematics.html
Para hacer coincidir objetos con partes del cuerpo, sería más recomendable el IK, la base no es muy dificil de aprender, pero la curva para dominarlo si que es mayor
docs.unity3d.comdocs.unity3d.com
Unity - Manual: Target Matching (6 kB)
https://docs.unity3d.com/Manual/TargetMatching.html

docs.unity3d.comdocs.unity3d.com
Unity - Manual: Inverse Kinematics (6 kB)
https://docs.unity3d.com/Manual/InverseKinematics.html


Otra alternativa, con uMotion, que lo enseñamos en la S5, también tiene funciones para aplicar IK respecto a los objetos, y te vienen unos demo-tutoriales bastante buenos