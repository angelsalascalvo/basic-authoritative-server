# Servidor Autoritario + Cliente. Unity 2D

Ejemplo de funcionamiento básico de movimiento de personajes para un videojuego multijugador online 2D con un servidor autoritario (Authoritative Server), los múltiples clientes envían sus entradas al servidor, este las procesa ejecutando su propia física y devuelve la posición de los diferentes jugadores a los clientes conectados. Toda esta comunicación se realiza a través de una comunicación <strong>UDP por socket</strong>.
<br><br>
La sincronización entre cliente y servidor se realiza a través un sistema de ticks. Con un tickrate de 50, en cada iteración se actúa del siguiente modo:
1. `Cliente` Envío de entradas efectuadas (inputs) al servidor.
2. `Cliente` Ejecución de movimiento en local (estimación) sobre el personaje principal a partir de las entradas efectuadas.
3. `Servidor` Procesamiento de entradas recibidas por parte de los clientes y ejecución de la física con el respectivo movimiento de los personajes.
4. `Servidor` Envío de las posiciones de todos los jugadores a los clientes conectados.
5. `Cliente` Corrección en la posición local del jugador principal respecto a la recibida por parte del servidor.
6. `Cliente` Interpolación de movimiento de los jugadores rivales con la posiciones recibidas por parte del servidor.

## Simulaciones
Implementados sistemas de simulación de <strong>latencia + perdida de paquetes</strong> para poder imitar un entorno real (nodos remotos) en ejecuciones locales.


## Múltiples clientes
Emplear la extensión [ParrelSync](https://github.com/VeriorPies/ParrelSync) para la creación de proyectos clones de unity a partir de enlaces simbólicos con los que poder simular la conexión de múltiples clientes sin necesidad de duplicar el proyecto completo.

## Bugs Conocidos
- Interpolación de jugadores rivales (en cliente) entrecortada / con interrupciones ante la perdida de datagramas en la comunicación.
- Correcciones bruscas e inmediatas en la posición del personaje principal
- Excepción sin controlar ante la desconexión de los nodos

## Pila Tecnológica
`Unity 2019.4.7`
