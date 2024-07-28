# Facepunch administrator tracker
> Made for official servers with the name **"**Facepunch**"**
>
> Reason for release: Got pissed cause admins guarding Low pop servers

# How to Build
> Visual Studio Community 2022
> 
> Should work with any net framework version

# How to use
- After running the program a folder will be created called "config"
- Make sure to setup steamPath and gamePath correctly so it can find the required files

## steamPath example:
- "C:\\Program Files (x86)\\Steam"

## gamePath example:
- "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Rust"

## How it works
- Using Steam Overlay data to determine if an Facepunch employee is networked to you
- If server population is < 100, admins are globally networked
- If server pop is higher than 100, you will only see admins that is spectating you or a player in your render

## Works best for
- When your own steam profile is set to Public, meaning game details Public, steam friend list can be hidden
- Servers with population below 100
- When server have more than 100 players, admin message might be delayed up to 5 minutes, still gives you a indication if you got manually banned

## My discord
`theminkman__`
