<a name="readme-top"></a>

<!-- PROJECT SHIELDS -->

<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![LGPL2 License][license-shield]][license-url]
<!-- CODE TEST -->
[<img src="https://sonarcloud.io/images/project_badges/sonarcloud-highlight.svg" alt="SonarQube Cloud" width="140"/>](https://sonarcloud.io/summary/new_code?id=NovaVoidHowl_CVR_Mods)\
![Lines of Code][sonarcloud-loc]
![Bugs][sonarcloud-bugs]
![Code Smells][sonarcloud-code-smells]
![Vulnerabilities][sonarcloud-vulnerabilities]
![Duplicated Lines (%)][sonarcloud-duplicated-lines]
![Reliability Rating][sonarcloud-reliability]
![Security Rating][sonarcloud-security]
![Maintainability Rating][sonarcloud-maintainability]

<!-- PROJECT LOGO -->

<br />
<div align="center">

<h3 align="center">NovaVoidHowl's CVR Mods</h3>

<p align="center">
    <br />
    <a href="https://github.com/NovaVoidHowl/CVR_Mods/blob/main/README.md"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/NovaVoidHowl/CVR_Mods/issues">Report Bug</a>
    ·
    <a href="https://github.com/NovaVoidHowl/CVR_Mods/issues">Request Feature</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about">About</a>
    </li>
    <li>
      <a href="#mod-list">Mod List</a>
    </li>
    <li>
      <a href="#building">Building</a>
      <ul>
        <li><a href="#set-cvr-folder-environment-variable">Set CVR Folder Environment Variable</a></li>
      </ul>
    </li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
    <li><a href="#project-tools">Project tools</a></li>
  </ol>
</details>

<!-- ABOUT -->

## About

Welcome to my little collection of mods for [ChilloutVR](https://abinteractive.net/),
feel free to leave bug reports or feature requests!

> [!NOTE]
> These modifications are unofficial and not supported by ChilloutVR or the ChilloutVR team.\
> Using this modification _might_ cause issues with performance, security or stability of the game.

> [!TIP]
> Should you wish to review the source code of these mods and compile them yourself please note that this project
> is setup for use with Visual Studio 2022 

<p align="right">(<a href="#readme-top">back to top</a>)</p>
<!-- MOD LIST -->

## Mods List

| Mod name | More Info                       | State | Latest Version Git | Latest Version CVRMG | Description                                                                            |
| -------- | ------------------------------- | :---: | :----------------: | :------------------: | -------------------------------------------------------------------------------------- |
| DataFeed | [README.md](DataFeed/README.md) | Ready |       0.9.0        |        0.8.0         | Exposes certain interface values as Avatar parameters and over REST and Websocket APIs |
| HRtoCVR  | [README.md](HRtoCVR/README.md)  | Ready |       0.1.20       |        0.1.19        | Provides Heart Rate values as avatar animator parameters                               |
| THtoCVR  | [README.md](THtoCVR/README.md)  | Ready |       0.1.0        |        0.0.4         | Temperature and/or humidity sensor info to avatar animator parameters.                 |

---

<!-- BUILDING -->

## Building

In order to build the mods in this project:

- (1) Install `NStrip.exe` from <https://github.com/BepInEx/NStrip> into this directory (or into your PATH). This tools
  converts all assembly symbols to public ones! If you don't strip the dlls, you won't be able to compile some mods.
- (2) If your ChilloutVR folder is `C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR` you can ignore this step.
  Otherwise follow the instructions bellow
  to [Set CVR Folder Environment Variable](#set-cvr-folder-environment-variable)
- (3) Run `copy_and_nstrip_dll.ps1` on the Power Shell. This will copy the required CVR, MelonLoader, and Mod DLLs into
  this project's `/.ManagedLibs`. Note if some of the required mods are not found, it will display the url from the CVR
  Modding Group API so you can download.

### Set CVR Folder Environment Variable

To build the project you need `CVRPATH` to be set to your ChilloutVR Folder, so we get the path to grab the libraries
we need to compile. By running the `copy_and_nstrip_dll.ps1` script that env variable is set automatically, but only
works if the ChilloutVR folder is on the default location `C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR`.

Otherwise you need to set the `CVRPATH` env variable yourself, you can do that by either updating the default path in
the `copy_and_nstrip_dll.ps1` and then run it, or manually set it via the windows menus.

#### Setup via editing copy_and_nstrip_dll.ps1

Edit `copy_and_nstrip_dll.ps1` and look the line bellow, and then replace the Path with your actual path.
`$cvrDefaultPath = "C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR"`

You'll probably need to restart your computer so the Environment Variable variable gets updated...

Now you're all set and you can go to the step (2) of the [Building](#building) instructions!

#### Setup via Windows menus

In Windows Start Menu, search for `Edit environment variables for your account`, and click `New` on the top panel.
Now you input `CVRPATH` for the **Variable name**, and the location of your ChilloutVR folder as the **Variable value**

By default this value would be `C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR`, but you wouldn't need to do
this if that was the case! Make sure it points to the folder where your `ChilloutVR.exe` is located.

Now you're all set and you can go to the step (2) of the [Building](#building) instructions! If you already had a power
shell window opened, you need to close and open again, so it refreshes the Environment Variables.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
<!-- CONTRIBUTING -->

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create.
Any contributions you make are **appreciated**. Please see [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

If you have a suggestion that would make this better, please fork the repo and create a pull request.
You can also simply open an issue with the tag "enhancement".

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- LICENSE -->

## License

Please see [LICENSE](LICENSE) for information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
<!-- CONTACT -->

## Contact

[@NovaVoidHowl](https://novavoidhowl.uk/)

Project Link: [https://cvr-mods.dev.novavoidhowl.uk](https://cvr-mods.dev.novavoidhowl.uk)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ACKNOWLEDGMENTS -->

## Acknowledgments

### [@kafeijao](https://github.com/kafeijao)

Thanks for your assistance with getting started on Mellon Loader Mods

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- PROJECT TOOLS -->

## Project tools

- VS Code, ide
- Pre-Commit, linting and error detection
- Github Copilot, Code error/issue analysis
- Sonar Qube Cloud, Code error/issue analysis

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->

<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[contributors-shield]: https://img.shields.io/github/contributors/NovaVoidHowl/CVR_Mods.svg?style=plastic
[contributors-url]: https://github.com/NovaVoidHowl/CVR_Mods/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/NovaVoidHowl/CVR_Mods.svg?style=plastic
[forks-url]: https://github.com/NovaVoidHowl/CVR_Mods/network/members
[issues-shield]: https://img.shields.io/github/issues/NovaVoidHowl/CVR_Mods.svg?style=plastic
[issues-url]: https://github.com/NovaVoidHowl/CVR_Mods/issues
[license-shield]: https://img.shields.io/badge/License-LGPL_2.1-blue
[license-url]: https://github.com/NovaVoidHowl/CVR_Mods/blob/master/LICENSE
[stars-shield]: https://img.shields.io/github/stars/NovaVoidHowl/CVR_Mods.svg?style=plastic
[stars-url]: https://github.com/NovaVoidHowl/CVR_Mods/stargazers
[sonarcloud-loc]: https://sonarcloud.io/api/project_badges/measure?project=NovaVoidHowl_CVR_Mods&metric=ncloc
[sonarcloud-bugs]: https://sonarcloud.io/api/project_badges/measure?project=NovaVoidHowl_CVR_Mods&metric=bugs
[sonarcloud-code-smells]: https://sonarcloud.io/api/project_badges/measure?project=NovaVoidHowl_CVR_Mods&metric=code_smells
[sonarcloud-vulnerabilities]: https://sonarcloud.io/api/project_badges/measure?project=NovaVoidHowl_CVR_Mods&metric=vulnerabilities
[sonarcloud-duplicated-lines]: https://sonarcloud.io/api/project_badges/measure?project=NovaVoidHowl_CVR_Mods&metric=duplicated_lines_density
[sonarcloud-reliability]: https://sonarcloud.io/api/project_badges/measure?project=NovaVoidHowl_CVR_Mods&metric=reliability_rating
[sonarcloud-security]: https://sonarcloud.io/api/project_badges/measure?project=NovaVoidHowl_CVR_Mods&metric=security_rating
[sonarcloud-maintainability]: https://sonarcloud.io/api/project_badges/measure?project=NovaVoidHowl_CVR_Mods&metric=sqale_rating
