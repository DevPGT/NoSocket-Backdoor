# NoSocket-Backdoor
Backdoor sem abertura de sockets.

Sistema de "Backdoor" que concede acesso remoto a nível administrativo no sistema que for executado, com uma engenharia diferenciada, mas com mesmo objetivo, controlar remotamente um dispositivo conectado à internet. Mais informações abaixo.

Backdoor convencional: Um backdoor convencional consiste em abrir portas na rede local e entrar em contato com a vítima numa rede externa por esta mesma porta, utilizando protocolos e sockets, o que é reconhecido por sistemas protetores.

Diferencial deste sistema: Por utilizar um sistema de cliente - servidor, ele é interpretado basicamente por uma conexão à internet, ou seja, participa das conexões do computador na própria porta comum da rede a 80, os dados entram e saem encriptografados por uma criptografia particular, sendo assim, não interpretáveis por outros programas e sistemas, esses mesmos dados não têm nenhuma porta ou socket aberto, ou seja, ele é tratado como uma simples conexão inocente, sendo indetectável por qualquer sistema protetor.

O algoritmo que executa no lado da vítima (backdoor) basicamente consiste em um loop de a cada x segundos consultar um servidor web hospedado pelo atacante em busca de novos comandos para si, em toda nova consulta é baixado os dados desencriptografados pela E2E do programa e então interpretados, verificados e manipulados. Quando esses comandos são encontrados, o código interpreta o comando, processa e retorna o resultado encriptografado novamente para o servidor web do atacante por uma requisição POST na porta 80.

O algoritmo de hospedagem web que recebe e envia os dados é escrito em php, ele apenas recebe informações e manipulam as mesmas, armazenando-as até seu uso ser concluído ou requerido.

O algoritmo do atacante (painel de comandos) consiste em automatizar os processos de enviar comandos para o terminal da vítima pela página php ou servidor web. Quando é enviado o comando, os dados passam para o servidor web via post encriptografado e são armazenados na web, assim quando seu respectivo alvo ou vítima solicitar por este dado, será excluído o comando para que novos comandos sejam executados, logo em seguida do comando enviado, o painel fica verificando por novos dados no servidor WEB, quando identificado novos dados, são desincriptografados, processados e exibidos na tela, sendo assim, seu funcionamento final resume-se em um terminal virtual e remoto com um intermediador web.
