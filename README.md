# Clients + Authoritative Server. Unity 2D

Ejemplo de funcionamineto básico de movimiento de personajes para un videojuego multijugador online 2D con un servidor autoritario (Authoritative Server), los multiples clientes envían
sus entradas al servidor, este las procesa ejecutando su propia física y devuelve la posicion de los diferentes jugadores a los clientes conectados. Toda esta comunicación se 
realiza a traves de una comunicación UDP por socket.
<br><br>
La sincronización entre cliente y servidor se realiza a través un sistema de ticks. Con un tickrate de 50, en cada iteración se actua del siguiente modo:
1. `Cliente` Envío de entradas efectuadas (inputs) al servidor.
2. `Cliente` Ejecución de movimiento en local (estimación) sobre el personaje principal a partir de las entradas efectuadas.
3. `Servidor` Procesasamiento de entradas recibidas por parte de los clientes y ejecución de fisica con movimiento de los personajes correspondientes.
4. `Servidor` Envío de las posiciones de todos los jugadores a los clientes conectados
5. `Cliente` Corrección en la posición local del jugador principal respecto a la recibida por parte del servidor.
6. `Cliente` Interpolación de movimiento de los jugadores rivales con la posiciones recibidas por parte del servidor.

## Simulaciones
Implementados sistemas de simulación de <strong>latencia + perdida de paquetes</strong> para poder imitar un entorno real (nodos remotos) en ejecuciones locales.


## Multiples clientes
Emplear la extensión [ParrelSync](https://github.com/VeriorPies/ParrelSync) para la creación de proyectos clones de unity a partir de enlaces simbólicos con los que poder simular la conexión de multiples clientes sin necesidad de duplicar el proyecto completo.

## Bugs Conocidos
- Interpolacion de jugadores rivales (en cliente) entrecortasa / con interrupciones ante la perdida de datagramas en la comunicación
- Correcciones bruscas e inmediatas en la posicion del personaje principal
- Excepción sin controlar ante la desconexión de los nodos

## Pila Tecnológica
`Unity 2019.4.7`
