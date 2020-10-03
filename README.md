<p align="center">
  <img src="https://github.com/guidospadavecchia/DolarBot/blob/master/design/images/dolar-logo-title.png" width="300px" height="300px">
</p>
  
<p align="center">
<i>El bot argentino de Discord para ver las cotizaciones del dólar y riesgo país.</i>  
</p>  

***    
## Status
[![Release](https://img.shields.io/github/v/release/guidospadavecchia/DolarBot?&label=version&style=flat-square)](https://github.com/guidospadavecchia/DolarBot/releases/latest)
[![License](https://img.shields.io/github/license/guidospadavecchia/DolarBot?color=orange&style=flat-square)](https://github.com/guidospadavecchia/DolarBot/blob/master/LICENSE)
[![Invite](https://img.shields.io/badge/Discord-invitar-7289DA)](https://discord.com/api/oauth2/authorize?client_id=752669185053818941&permissions=321600&scope=bot)
[![Discord](https://img.shields.io/discord/752312522769694780?color=7289DA&label=Discord%20Support%20Server&style=flat-square)](https://discord.gg/tCkbjuM)
[![Issues](https://img.shields.io/github/issues/guidospadavecchia/DolarBot?style=flat-square)](https://github.com/guidospadavecchia/DolarBot/issues)
[![PullRequests](https://img.shields.io/github/issues-pr-closed/guidospadavecchia/DolarBot?style=flat-square)](https://github.com/guidospadavecchia/DolarBot/pulls)  

## Descripción  
**DolarBot** te permite ver todas las cotizaciones del dólar en un mismo lugar, y desde la comodidad de tu servidor de Discord. El prefijo para invocar cualquier comando con el bot es `$`. A su vez, la mayoría de los comandos tienen un atajo que permite invocarlos más rápidamente.  
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

### Cotizaciones
- \[`$bancos`]: Muestra la lista de bancos disponibles para obtener sus cotizaciones.  
- \[`$dolar` | `$d`]: Muestra todas las cotizaciones del dólar disponibles.  
- \[`$dolar` | `$d`] `<banco>`: Muestra la cotización del dólar oficial del banco especificado (Ver `$bancos`).  
- \[`$dolar` | `$d`] `bancos`: Muestra la cotización de todos los bancos (Ver `$bancos`).  
- \[`$dolaroficial` | `$do`]: Muestra la cotización del dólar oficial del Banco Nación.  
- \[`$dolarahorro` | `$da`]: Muestra la cotización del dólar oficial más impuesto P.A.I.S. y retención de ganancias.  
- \[`$dolarblue` | `$db`]: Muestra la cotización del dólar blue.  
- \[`$dolarpromedio` | `$dp`]: Muestra el promedio de las cotizaciones bancarias del dólar oficial.  
- \[`$dolarbolsa` | `$dbo`]: Muestra la cotización del dólar bolsa (MEP).  
- \[`$contadoconliqui` | `$ccl`]: Muestra la cotización del dólar contado con liquidación.  
- \[`$riesgopais` | `$rp`]: Muestra el valor del riesgo país.  

### Información
- \[`$hora` | `$date`]: Muestra la fecha y hora del bot y del servidor donde se aloja.  
- \[`$sid`]: Muestra el ID del servidor de Discord actual.  
- \[`$ping`]: Muestra la latencia del bot de Discord.  
- \[`$invite` | `$invitar`]: Devuelve el link de invitación del bot en Discord.  
- \[`$bot`]: Muestra información acerca del bot.  

## Librerías y APIS
- [Discord.NET](https://github.com/discord-net/Discord.Net)
- [Joe4evr/Discord.Addons](https://github.com/Joe4evr/Discord.Addons)
- [guidospadavecchia/Discord.Addons.Interactive](https://github.com/guidospadavecchia/Discord.Addons.Interactive) (fork propio de [PassiveModding/Discord.Addons.Interactive](https://github.com/PassiveModding/Discord.Addons.Interactive))
- [Castrogiovanni20/api-dolar-argentina](https://github.com/Castrogiovanni20/api-dolar-argentina)

## Contribuciones
Reportá problemas o sugerencias [acá](https://github.com/guidospadavecchia/DolarBot/issues).  
Si deseás contribuir, podés abrir un [pull request](https://github.com/guidospadavecchia/DolarBot/pulls).  

¿Te gusta **DolarBot**? Podes impulsar su avance y mantenimiento con una [pequeña contribución](https://www.mercadopago.com.ar/checkout/v1/redirect?preference-id=644604751-7a01236a-d22c-49f9-9194-f77c58485af1).

## Autor
Guido Spadavecchia (Contacto: guido.spadavecchia@gmail.com).  

## Licencia
**DolarBot** está licenciado bajo la [MIT License](https://github.com/guidospadavecchia/SteamBuddy/blob/master/LICENSE).

## 
<p align="center">
  <img src="http://ForTheBadge.com/images/badges/built-with-love.svg">
</p>
