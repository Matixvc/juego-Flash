# Guia del proyecto Unity 2D URP

## 1. Flujo general

1. `GameBootstrap` se ejecuta al cargar la escena.
2. `GameManager` controla el estado de la partida, el tiempo, la pausa y los jefes.
3. `PlayerController2D` mueve al jugador y administra vida, dash y muerte.
4. `StoneAttack2D` ejecuta el pisoton y el dano durante el dash.
5. `EnemySpawner2D` crea enemigos y jefes.
6. `Enemy2D` persigue, recibe dano, muere y suelta orbes.
7. `ExpOrbe` entrega experiencia a `RunProgressSO`.
8. `UpgradeManager` muestra las mejoras al subir de nivel.
9. `HUDManager` actualiza vida, experiencia, esencia, tier, pausa y Game Over.

## 2. Objetos que deben existir en la escena

### Player

Debe tener en el mismo GameObject:

- `Transform`
- `SpriteRenderer`
- `Rigidbody2D`: gravedad 0, rotacion congelada.
- `CircleCollider2D`.
- `PlayerController2D`.
- `PlayerRuntimeStats` con `PlayerData.asset` asignado.
- `StoneAttack2D`.

No agregues dos `PlayerController2D` ni dos `PlayerRuntimeStats` al mismo objeto.

### Main Camera

Debe tener:

- `Camera` en modo ortografico.
- `CamaraSeguimiento`.
- Un solo `CamaraTremor`.

`GameBootstrap` instala los componentes de camara que falten. Si hay dos `CamaraTremor`, elimina el duplicado manualmente.

### Managers

Puedes dejar estos objetos en `SampleScene` o permitir que `GameBootstrap` los cree:

- `GameManager`.
- `EnemySpawner2D`.
- `UpgradeManager`.
- Un `Canvas` con `HUDManager`.

No dupliques managers. `GameBootstrap` tiene un fallback de auto-creacion para escenas incompletas.

### Enemy_Escarabajo

El prefab valido es `Assets/_Project/Enemy_Escarabajo.prefab`. Debe conservar:

- `Rigidbody2D`.
- `CircleCollider2D` sin `Is Trigger`.
- `Enemy2D` con `EnemyData.asset` y el prefab de `ExpOrbe`.

`SpawnConfig.asset` ya referencia este prefab.

### Enemy_Jefe

Debe tener `Rigidbody2D`, collider no trigger y `EnemyBoss`. En `SpawnConfig.asset` asigna el prefab del jefe y `JefeData.asset`.

## 3. Datos ScriptableObject

Los datos usados por el juego estan en `Assets/Resources`, por lo que el bootstrap puede cargarlos aunque la escena no tenga referencias directas:

- `PlayerData.asset`: estadisticas iniciales del jugador.
- `RunProgress.asset`: experiencia, nivel, esencia, kills y tier.
- `SpawnConfig.asset`: ritmo de oleadas, arena, prefabs y jefes.
- `UpgradePool.asset`: lista de mejoras disponibles.

La copia de runtime es importante: `PlayerRuntimeStats` modifica valores durante una partida sin modificar `PlayerData.asset`.

## 4. Controles actuales

- `WASD` o flechas: mover la roca.
- `Espacio` o boton sur del gamepad: dash.
- `J` o clic izquierdo: pisoton.
- `Escape`: pausar o continuar.
- Boton `REINICIAR`: recarga la escena activa.
- Al subir de nivel: elegir una carta de mejora con clic.

El proyecto usa el Input System nuevo. No mezcles este codigo con llamadas antiguas de `Input.GetKey`.

## 5. Uso de cada grupo de scripts

### Core

- `GameBootstrap`: punto de entrada; asegura datos, managers, canvas y camara.
- `GameManager`: estados `Jugando`, `Pausa`, `SeleccionMejora`, `EventoJefe` y `GameOver`.
- `BootstrapLinks`: compatibilidad para escenas antiguas; normalmente no se configura manualmente.
- `PlayerDataSO`: valores base del jugador.
- `PlayerRuntimeStats`: valores modificables durante la run.
- `EnemyDataSO`: valores base de un enemigo.
- `SpawnConfigSO`: reglas del spawner y referencias de prefabs.
- `RunProgressSO`: experiencia, nivel, tiempo, tier y recompensas.

### Jugador y combate

- `PlayerController2D`: movimiento, dash, dano, invulnerabilidad y muerte.
- `StoneAttack2D`: pisoton y dano por contacto durante dash.

### Enemigos

- `EnemySpawner2D`: genera oleadas y activa el evento del jefe.
- `Enemy2D`: persecucion, separacion, knockback, dano y drops.
- `EnemyBoss`: embestida, invocacion de minis y aviso de jefe derrotado.
- `ExpOrbe`: magnetismo y entrega de experiencia.

### Mejoras

- `UpgradePoolSO`: pool de cartas.
- `UpgradeSO`: una mejora y sus efectos.
- `UpgradeManager`: selecciona y aplica mejoras.
- `UpgradeUI`: pinta las cartas y llama a `UpgradeManager.ElegirMejora`.
- `IUpgradeSelector`: contrato entre el manager y la UI.

### UI y VFX

- `HUDManager`: crea y actualiza el HUD desde codigo.
- `UIHelper` y `RecursosUI`: utilidades para construir controles.
- `HitStop`: pausa corta de impactos.
- `FlashPantalla`: flash al recibir dano.
- `CamaraTremor`: temblor de camara.
- `CamaraSeguimiento`: sigue al jugador y limita la arena.
- `NumeroDanio`: numeros flotantes.
- `RastroDash`: rastro visual del dash.
- `VfxUtil` y `VfxPool`: efectos reutilizables.

## 6. Orden recomendado para configurar una escena nueva

1. Crea una escena 2D y agrega una `Main Camera` ortografica.
2. Arrastra el prefab del jugador y verifica `PlayerData.asset`.
3. Agrega `GameBootstrap` a un objeto vacio llamado `_GameBootstrap`, o deja que se cree automaticamente.
4. Verifica que `Assets/Resources` contenga los cuatro ScriptableObjects indicados.
5. Verifica los prefabs en `SpawnConfig.asset`.
6. Pulsa Play y confirma que el HUD aparece sin crear Canvas duplicados.
7. Prueba movimiento, dash y pisoton.
8. Espera una oleada y confirma que los enemigos siguen al jugador.
9. Elimina un enemigo y recoge su orbe.
10. Sube de nivel y elige una mejora.
11. Pulsa Escape, confirma que nada se mueve y continua.
12. Para probar jefe, reduce temporalmente `TiempoEntreJefes` en `SpawnConfig.asset`; luego devuelve `270` segundos.

## 7. Checklist de diagnostico

- Si no se mueve el jugador: comprobar `PlayerRuntimeStats`, `PlayerData.asset` y que el estado sea `Jugando`.
- Si no aparecen enemigos: comprobar `SpawnConfig.asset`, `prefabEscarabajo`, `Enemy2D` y `maxEnemigosActivos`.
- Si no hay experiencia: comprobar collider trigger del orbe y `GameManager.RunProgress`.
- Si no aparecen mejoras: comprobar `UpgradePool.asset`, la lista `mejorasDisponibles` y que cada carta tenga efectos.
- Si los botones no responden: comprobar que exista un `EventSystem`; `UIHelper` lo crea al construir botones.
- Si la pausa termina sola: comprobar que no haya otro script escribiendo `Time.timeScale`.
- Si la camara tiembla demasiado: dejar un solo `CamaraTremor`.

## 8. Validacion realizada

- `get_errors` no reporta errores en los scripts.
- `dotnet build "juego Flash.slnx" --no-restore` finaliza correctamente tras restaurar los assets de build.
- Se corrigio la configuracion temprana de datos entre `GameBootstrap` y los managers.
- Se corrigio el desbloqueo del pisoton cuando faltan stats.
- `HitStop` respeta una pausa aplicada mientras el efecto esta activo.
- `PlayerRuntimeStats.Reiniciar()` limpia `Leech` para que no pase a otra partida.

La validacion final debe hacerse en Play Mode dentro de Unity siguiendo la checklist anterior.
