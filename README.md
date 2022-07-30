<p align="center">
  <img src="https://github.com/guidospadavecchia/DolarBot/blob/master/design/images/dolarbot-logo-title.png" width="600px">
</p>

*** 
<p align="center">
<i>El bot argentino de Discord para ver las cotizaciones del dólar, euro, real, indicadores y más!</i>  
</p>  

## Status
[![Status](https://top.gg/api/widget/status/752669185053818941.svg)](https://top.gg/bot/752669185053818941)
[![Servers](https://top.gg/api/widget/servers/752669185053818941.svg)](https://top.gg/bot/752669185053818941)
[![Release](https://img.shields.io/github/v/release/guidospadavecchia/DolarBot?&label=version&style=flat-square)](https://github.com/guidospadavecchia/DolarBot/releases/latest)
[![API](https://img.shields.io/github/package-json/v/guidospadavecchia/DolarBot-Api?color=teal&label=api&style=flat-square)](https://dolarbot-api.herokuapp.com/)
[![Invite](https://img.shields.io/badge/Discord-invitar-7289DA)](https://discord.com/api/oauth2/authorize?client_id=752669185053818941&permissions=321600&scope=bot)
[![Discord](https://img.shields.io/discord/752312522769694780?color=7289DA&label=Support%20Server&style=flat-square)](https://discord.gg/tCkbjuM)
[![License](https://img.shields.io/github/license/guidospadavecchia/DolarBot?color=orange&style=flat-square)](https://github.com/guidospadavecchia/DolarBot/blob/master/LICENSE)  

## Descripción  
**DolarBot** te permite ver todas las cotizaciones del **Dólar**, **Euro**, **Real**, metales preciosos, criptomonedas, indicadores económicos como riesgo país, reservas del BCRA y mucho más, en un mismo lugar, y desde la comodidad de tu servidor de Discord. El prefijo para invocar cualquier comando con el bot es `/`. Para ver todos los comandos disponibles, ejecutá `/ayuda`.

## Discord
Podés invitar al bot a tu servidor haciendo [click acá](https://discord.com/api/oauth2/authorize?client_id=752669185053818941&permissions=321600&scope=bot). Las versiones ofrecidas en este repositorio existen únicamente para aquellos que deseen hostear su propia instancia.

## Comandos
A continuación se listan los comandos disponibles:

### Ayuda
- \[`/ayuda`]: Muestra los comandos disponibles.  
- \[`/ayuda`] `<comando>`: Muestra información sobre un comando particular.  

### Cotizaciones del dólar
- \[`/dolar`]: Muestra todas las cotizaciones del dólar.  
- \[`/dolar`] `<tipo>`: Muestra la cotización del dólar según el tipo especificado.  
- \[`/dolar-bancos`]: Muestra todas las cotizaciones bancarias del dólar.  
- \[`/dolar-bancos`] `<banco>`: Muestra la cotización del dólar del banco especificado.  

### Cotizaciones del Euro
- \[`/euro`]: Muestra todas las cotizaciones del Euro.  
- \[`/euro`] `<tipo>`: Muestra la cotización del Euro según el tipo especificado.  
- \[`/euro-bancos`]: Muestra todas las cotizaciones bancarias del Euro.  
- \[`/euro-bancos`] `<banco>`: Muestra la cotización del Euro del banco especificado. 

### Cotizaciones del Real
- \[`/real`]: Muestra todas las cotizaciones del Real.  
- \[`/real`] `<tipo>`: Muestra la cotización del Real según el tipo especificado.  
- \[`/real-bancos`]: Muestra todas las cotizaciones bancarias del Real.  
- \[`/real-bancos`] `<banco>`: Muestra la cotización del Real del banco especificado. 

### Monedas del mundo
- \[`/cotizacion`]: Muestra la lista de todas las monedas del mundo disponibles.  
- \[`/cotizacion`] `<codigo>`: Muestra la cotización actual de una moneda según su código.  
- \[`/historico`] `<moneda>` `<desde>` `<hasta>`: Muestra valores históricos entre fechas para una moneda determinada (ver `/cotizacion`).  

### Metales
- \[`/oro`]: Muestra la cotización internacional del Oro.  
- \[`/plata`]: Muestra la cotización internacional de la Plata.  
- \[`/cobre`]: Muestra la cotización internacional del Cobre.  

### Criptomonedas
- \[`/crypto`]: Muestra la lista de todos los códigos de criptomonedas disponibles.  
- \[`/crypto`] `<criptomoneda>`: Muestra la cotización actual de una criptomoneda determinada según su código.  

### Indicadores
- \[`/riesgopais`]: Muestra el valor del riesgo país.  
- \[`/reservas`]: Muestra la cantidad total de reservas en dólares del BCRA a la fecha.  
- \[`/circulante`]: Muestra la cantidad total de pesos en circulación a la fecha.  

### Cotizaciones Venezuela
- \[`/bolivar-dolar`]: Muestra las distintas cotizaciones del dólar en Venezuela, expresadas en bolívares.  
- \[`/bolivar-euro`]: Muestra las distintas cotizaciones del Euro en Venezuela, expresadas en bolívares.  

### Evolución
- \[`/evolucion`] `<cotizacion>`: Muestra la evolución mensual anualizada de las distintas cotizaciones disponibles.

### Información
- \[`/hora`]: Muestra la fecha y hora del bot y del servidor donde se aloja.  
- \[`/server-id`]: Muestra el ID del servidor de Discord actual.  
- \[`/ping`]: Muestra la latencia del bot de Discord.  
- \[`/invitar`]: Devuelve el link de invitación del bot en Discord.  
- \[`/votar`]: Muestra el link para votar por **DolarBot** en **top.gg**.  
- \[`/bot`]: Muestra información acerca del bot.  
- \[`/status`]: Muestra el estado actual del bot y sus servicios.  

### Otros
- \[`/bancos`]: Muestra la lista de bancos disponibles para cada una de las monedas principales (Dólar, Euro y Real).  
- \[`/frase`]: Muestra una frase célebre acerca de la economía argentina.  

## Librerias
- [Discord.Net](https://github.com/discord-net/Discord.Net)
- [Fergun.Interactive](https://github.com/d4n3436/Fergun.Interactive)
- [DBL-dotnet-Library](https://github.com/top-gg/dotnet-sdk)

## APIS
- [DolarBot-Api](https://github.com/guidospadavecchia/DolarBot-Api)

## Contribuciones
Reportá problemas en la sección [issues](https://github.com/guidospadavecchia/DolarBot/issues), y utilizá la sección [discussions](https://github.com/guidospadavecchia/DolarBot/discussions) para preguntas y sugerencias.  
Si deseás contribuir, podés abrir un [pull request](https://github.com/guidospadavecchia/DolarBot/pulls).  

¿Te gusta **DolarBot**? Apoyalo [votándolo en top.gg](https://top.gg/bot/752669185053818941/vote).  
También podes impulsar su avance y mantenimiento con una [pequeña contribución](http://paypal.me/guidospadavecchia).

## Autor
[Guido Spadavecchia](https://github.com/guidospadavecchia) (Contacto: guido.spadavecchia@gmail.com).  

## Licencia
**DolarBot** es *open-source*, libre y gratuito. Está licenciado bajo la [MIT License](https://github.com/guidospadavecchia/DolarBot/blob/master/LICENSE).  

## Renuncia de responsabilidad
**DolarBot** recopila y muestra información de distintas fuentes, quienes son responsables de mantenerla actualizada. Por este motivo, no es posible garantizar la disponibilidad, relevancia y fidelidad de la información provista por terceros y ofrecida a través de **DolarBot**.

## 
<p align="center">
  <img src="http://ForTheBadge.com/images/badges/built-with-love.svg">
</p>
