# Configurações
- crie uma pasta no drive e obtenha o Id dela (fica na url), adicione esse id no appsettings.json (como estamos usando OAuth como credencial, não é necessário tornar a pasta publica ou compartilha-la com outra conta)
- abrir o google cloud console
- crie um projeto
- abilitar a API do Google Drive na aba bibliotecas
- gerar uma credencial do tipo OAuth
- configure cliente(talvez opciona) e url de redirecionamento(importante para obter o refresh token) 
- obter o client_id e o client_secret das credenciais pelo google cloud e adicionar essas informações no appsettings.json
- chamar o endpoint de login uma unica vês para obter o refresh token (será gerado um arquivo com o nome google-token.json, dentro dele estará o refresh)
- adicionar esse refresh token no app settings.json.
- videos que me ajudaram a configurar o Cloud:
  - https://youtu.be/Z37aIs1M--A?si=vldUINpqLIb45OL4
  - https://youtu.be/HCT6VA92oCY?si=r2U7k9g1rN8vPBHz
 <br>
 
  - OBS: tentei configurar para usar credencial de serviço também. Mas não deu certo, o código rodou quando usei a credencial de um colega meu, mas não rodou usando minha credencial :exploding_head: :face_with_spiral_eyes: :man_shrugging: se você sabe porque, comparilhe conosco seu conhecimento codistico
  
