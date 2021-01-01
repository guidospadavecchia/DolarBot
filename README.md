<p align="center">
  <img src="https://github.com/guidospadavecchia/DolarBot/blob/master/design/images/dolar-logo-title.png" width="300px" height="300px">
</p>
  
<p align="center">
<i>El bot argentino de Discord para ver las cotizaciones del dólar, euro, real, indicadores y más!</i>  
</p>  

***    
## Status
[![Status](https://top.gg/api/widget/status/752669185053818941.svg)](https://top.gg/bot/752669185053818941)
[![Servers](https://top.gg/api/widget/servers/752669185053818941.svg)](https://top.gg/bot/752669185053818941)
[![Release](https://img.shields.io/github/v/release/guidospadavecchia/DolarBot?&label=version&style=flat-square)](https://github.com/guidospadavecchia/DolarBot/releases/latest)
[![API](https://img.shields.io/github/package-json/v/guidospadavecchia/DolarBot-Api?color=teal&label=api&style=flat-square)](https://dolarbot-api.herokuapp.com/)
[![Invite](https://img.shields.io/badge/Discord-invitar-7289DA)](https://discord.com/api/oauth2/authorize?client_id=752669185053818941&permissions=321600&scope=bot)
[![Discord](https://img.shields.io/discord/752312522769694780?color=7289DA&label=Discord%20Support%20Server&style=flat-square)](https://discord.gg/tCkbjuM)
[![License](https://img.shields.io/github/license/guidospadavecchia/DolarBot?color=orange&style=flat-square)](https://github.com/guidospadavecchia/DolarBot/blob/master/LICENSE)  

## Descripción  
**DolarBot** te permite ver todas las cotizaciones del dólar, Euro, Real y distintos indicadores económicos como riesgo país, reservas del BCRA y otros, en un mismo lugar, y desde la comodidad de tu servidor de Discord. El prefijo para invocar cualquier comando con el bot es `$`. A su vez, la mayoría de los comandos tienen un atajo que permite invocarlos más rápidamente.  
La cantidad de comandos que puede ejecutar cada usuario está limitada a **1 comando cada 5 segundos**, ayudando a evitar problemas de performance. Si ves que **DolarBot** no responde a tus comandos en un tiempo superior, es probable que haya algún problema temporal.

## Discord
Podés invitar al bot a tu servidor haciendo [click acá](https://discord.com/api/oauth2/authorize?client_id=752669185053818941&permissions=321600&scope=bot). Las versiones ofrecidas en este repositorio existen únicamente para aquellos que deseen hostear su propia instancia.

## Comandos
A continuación se listan los comandos disponibles:

### Ayuda
- \[`$ayuda` | `$a`]: Muestra los comandos disponibles.  
- \[`$ayudadm` | `$adm`]: Envía la ayuda por mensaje privado.  
- \[`$ayuda` | `$a`] `<comando>`: Muestra información sobre un comando particular.  
- \[`$ayudadm` | `$adm`] `<comando>`: Envía la ayuda de un comando particular por mensaje privado.  

### Cotizaciones del dólar
- \[`$dolar` | `$d`]: Muestra todas las cotizaciones del dólar disponibles.  
- \[`$dolar` | `$d`] `<banco>`: Muestra la cotización del dólar oficial para un banco puntual (Ver `$bancos dolar`).  
- \[`$dolar` | `$d`] `bancos`: Muestra la cotización de todos los bancos (Ver `$bancos`).  
- \[`$dolaroficial` | `$do`]: Muestra la cotización del dólar oficial del Banco Nación.  
- \[`$dolarahorro` | `$da`]: Muestra la cotización del dólar ahorro (dólar oficial más impuesto P.A.I.S. y retención de ganancias).  
- \[`$dolarahorro` | `$da`] `<banco>`: Muestra la cotización del dólar ahorro para un banco puntual (Ver `$bancos dolar`).  
- \[`$dolarblue` | `$db`]: Muestra la cotización del dólar blue.  
- \[`$dolarpromedio` | `$dp`]: Muestra el promedio de las cotizaciones bancarias del dólar oficial.  
- \[`$dolarbolsa` | `$dbo`]: Muestra la cotización del dólar bolsa (MEP).  
- \[`$contadoconliqui` | `$ccl`]: Muestra la cotización del dólar contado con liquidación.  

### Cotizaciones del Euro
- \[`$euro` | `$e`]: Muestra todas las cotizaciones del Euro oficial para los bancos disponibles.  
- \[`$euro` | `$e`] `<banco>`: Muestra la cotización del Euro oficial para un banco puntual (Ver `$bancos euro`).  
- \[`$euroahorro` | `$ea`]: Muestra todas las cotizaciones del Euro ahorro para los bancos disponibles (Euro oficial más impuesto P.A.I.S. y retención de ganancias).  
- \[`$euroahorro` | `$ea`] `<banco>`: Muestra la cotización del Euro ahorro para un banco puntual (Ver `$bancos euro`).  

### Cotizaciones del Real
- \[`$real` | `$r`]: Muestra todas las cotizaciones del Real oficial para los bancos disponibles.  
- \[`$real` | `$r`] `<banco>`: Muestra la cotización del Real oficial para un banco puntual (Ver `$bancos real`).  
- \[`$realahorro` | `$ra`]: Muestra todas las cotizaciones del Real ahorro para los bancos disponibles (Real oficial más impuesto P.A.I.S. y retención de ganancias).  
- \[`$realahorro` | `$ra`] `<banco>`: Muestra la cotización del Real ahorro para un banco puntual (Ver `$bancos real`).  

### Indicadores
- \[`$riesgopais` | `$rp`]: Muestra el valor del riesgo país.  
- \[`$reservas` | `$rs`]: Muestra la cantidad total de reservas en dólares del BCRA a la fecha.  
- \[`$circulante` | `$c`]: Muestra la cantidad total de pesos en circulación a la fecha.  

### Evolución
- \[`$evolucion` | `$ev`] `<cotizacion>`: Muestra la evolución mensual anualizada de las distintas cotizaciones disponibles.  

### Información
- \[`$hora` | `$date`]: Muestra la fecha y hora del bot y del servidor donde se aloja.  
- \[`$sid`]: Muestra el ID del servidor de Discord actual.  
- \[`$ping`]: Muestra la latencia del bot de Discord.  
- \[`$invite` | `$invitar`]: Devuelve el link de invitación del bot en Discord.  
- \[`$bot`]: Muestra información acerca del bot.  
- \[`$status`]: Muestra el estado actual del bot y sus servicios.  
- \[`$votar` | `$vote`]: Muestra el link para votar por **DolarBot** en **top.gg**.  

### Otros
- \[`$monedas`]: Muestra la lista de monedas soportadas.  
- \[`$bancos`]: Muestra la lista de bancos disponibles para cada una de las monedas (Ver `$monedas`).  
- \[`$bancos`] `<moneda>`: Muestra la lista de bancos disponibles la moneda especificada (Ver `$monedas`).  
- \[`$frase` | `$f`]: Muestra una frase célebre acerca de la economía argentina.  

## Librerías y APIS
- [Discord.NET](https://github.com/discord-net/Discord.Net)
- [Joe4evr/Discord.Addons](https://github.com/Joe4evr/Discord.Addons)
- [guidospadavecchia/DolarBot-Api](https://github.com/guidospadavecchia/DolarBot-Api)
- [guidospadavecchia/Discord.Addons.Interactive](https://github.com/guidospadavecchia/Discord.Addons.Interactive)
- [top-gg/DBL-dotnet-Library](https://github.com/top-gg/dotnet-sdk)

## Contribuciones
Reportá problemas en la sección [issues](https://github.com/guidospadavecchia/DolarBot/issues), y utilizá la sección [discussions](https://github.com/guidospadavecchia/DolarBot/discussions) para preguntas y sugerencias.  
Si deseás contribuir, podés abrir un [pull request](https://github.com/guidospadavecchia/DolarBot/pulls).  

¿Te gusta **DolarBot**? Apoyalo votándolo en [top.gg](https://top.gg/bot/752669185053818941/vote).  
También podes impulsar su avance y mantenimiento con una [pequeña contribución](https://www.mercadopago.com.ar/checkout/v1/redirect?preference-id=644604751-7a01236a-d22c-49f9-9194-f77c58485af1).

## Autor
Guido Spadavecchia (Contacto: guido.spadavecchia@gmail.com).  

## Licencia
**DolarBot** es *open-source*, libre y gratuito. Está licenciado bajo la [MIT License](https://github.com/guidospadavecchia/DolarBot/blob/master/LICENSE).

## 
<p align="center">
  <img src="http://ForTheBadge.com/images/badges/built-with-love.svg">
</p>
