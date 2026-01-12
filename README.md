# Configurações
- abrir o google cloud console
- crie um projeto
- abilitar a API do Google Drive na aba bibliotecas
- gerar uma credencial do tipo OAuth
- configure cliente(talvez opciona) e url de redirecionamento(importante para obter o refresh token) 
- obter o client_id e o client_secret das credenciais pelo google cloud e adicionar essas informações no appsettings.json junto com o id da pasta do drive
- chamar o endpoint de login uma unica vês para obter o refresh token (será gerado um arquivo com o nome google-token.json, dentro dele estará o refresh)
- adicionar esse refresh token no app settings.json.
- videos que me ajudaram a configurar o Cloud: https://youtu.be/Z37aIs1M--A?si=vldUINpqLIb45OL4