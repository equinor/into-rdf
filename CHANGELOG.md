# Changelog

## [1.1.1](https://github.com/equinor/spine-splinter/compare/v1.1.0...v1.1.1) (2022-08-23)


### Bug Fixes

* to eager doing cleanup. Readded a line of code ([2bcff24](https://github.com/equinor/spine-splinter/commit/2bcff24c3607fd2d3ba87b9631de9ce820ef2faa))
* trigger release ([b57f9d5](https://github.com/equinor/spine-splinter/commit/b57f9d5e08da1be6bc9789db852ded3c7774e921))

## [1.1.0](https://github.com/equinor/spine-splinter/compare/v1.0.0...v1.1.0) (2022-08-23)


### Features

* 75640 update excel2 rdf to generate provenance from fuseki ([#70](https://github.com/equinor/spine-splinter/issues/70)) ([b55fa00](https://github.com/equinor/spine-splinter/commit/b55fa00c0a1afb99c337a49b991f0346403e0a64))
* AML Ingest ([#74](https://github.com/equinor/spine-splinter/issues/74)) ([331e7e1](https://github.com/equinor/spine-splinter/commit/331e7e1a29670e42b6f1097891f587ab78ed4583))


### Bug Fixes

* 🐛 Add missing linuxFxVersion ([82098dc](https://github.com/equinor/spine-splinter/commit/82098dcf7288754766cd723aa5fca589e98cdb7c))
* 🐛 different linuxFxVersion for webapp and functionapp ([#85](https://github.com/equinor/spine-splinter/issues/85)) ([6d80c46](https://github.com/equinor/spine-splinter/commit/6d80c4692455fc3cfcd112ac932014e5237f6af7))
* 🐛 Move substitution of baseUrl from bicep to action ([#84](https://github.com/equinor/spine-splinter/issues/84)) ([7637687](https://github.com/equinor/spine-splinter/commit/7637687ef7bc01f42ae63e3d318b56f80efa0f66))
* Fixed bug with getting previous revisions from the Fuseki ([#64](https://github.com/equinor/spine-splinter/issues/64)) ([abc80b9](https://github.com/equinor/spine-splinter/commit/abc80b9ca31559e66f5b257a17bb126cf60e5033))


### Other

* 🤖 update packages ([#77](https://github.com/equinor/spine-splinter/issues/77)) ([4dedde3](https://github.com/equinor/spine-splinter/commit/4dedde3caae4e5c8aaf0db47eb54f33a9fc12d5b))
* 75634 generalizing splinter transformations ([#69](https://github.com/equinor/spine-splinter/issues/69)) ([0d2013b](https://github.com/equinor/spine-splinter/commit/0d2013b92f189ba443d3928d9af0f40457d5770f))
* Added readme for running the various Azure functions locally ([#79](https://github.com/equinor/spine-splinter/issues/79)) ([5193f1f](https://github.com/equinor/spine-splinter/commit/5193f1fd4dd741cdcbcfd2cb99c462ffa998eed1))
* **deps:** bump Azure/functions-action from 1.4.6 to 1.4.7 ([#83](https://github.com/equinor/spine-splinter/issues/83)) ([bffe371](https://github.com/equinor/spine-splinter/commit/bffe371fa6fd5674c98bcb36c1c5ec6d2bf54696))
* **deps:** bump Microsoft.ApplicationInsights.AspNetCore ([#68](https://github.com/equinor/spine-splinter/issues/68)) ([19072d4](https://github.com/equinor/spine-splinter/commit/19072d48e18fd27a84bc25126dc07b6e6f75f435))
* **deps:** bump Serilog.AspNetCore from 6.0.0 to 6.0.1 ([#65](https://github.com/equinor/spine-splinter/issues/65)) ([942f6b4](https://github.com/equinor/spine-splinter/commit/942f6b41bce2c1a37dd1755930de38dd00fe3641))
* **deps:** bump Swashbuckle.AspNetCore from 6.3.2 to 6.4.0 ([#66](https://github.com/equinor/spine-splinter/issues/66)) ([8164e18](https://github.com/equinor/spine-splinter/commit/8164e188729ecc65a0490978e75ced7552484850))


### CI/CD

* 🎡 create own storage account for az function ([#82](https://github.com/equinor/spine-splinter/issues/82)) ([cb70eb2](https://github.com/equinor/spine-splinter/commit/cb70eb2a7da03a893948a0214580cbc6b1f7b2da))
* 🎡 setup mel fuseki in az function bicep ([#81](https://github.com/equinor/spine-splinter/issues/81)) ([eebaa46](https://github.com/equinor/spine-splinter/commit/eebaa464f4dec0d368cd2b26801a4e895817ea50))

## 1.0.0 (2022-07-19)


### Features

* 🎸 Add Application Insights logging ([#43](https://github.com/equinor/spine-splinter/issues/43)) ([7a61c76](https://github.com/equinor/spine-splinter/commit/7a61c764ab781a94876a2aba2f752769f0a33528))
* 🎸 add ingest controller ([#3](https://github.com/equinor/spine-splinter/issues/3)) ([846e9c8](https://github.com/equinor/spine-splinter/commit/846e9c8f67590d69e59de4f3c0c171f36cb78f9a))
* 71318/align shipweight and mel transformations ([#31](https://github.com/equinor/spine-splinter/issues/31)) ([1d4af63](https://github.com/equinor/spine-splinter/commit/1d4af636b9436e06166db16131ff6b0b4476d2cb))
* add azure function ([#32](https://github.com/equinor/spine-splinter/issues/32)) ([8fb2c26](https://github.com/equinor/spine-splinter/commit/8fb2c26c43fb6a252221efe019525fb12d785d7d))
* add service bus event ([#49](https://github.com/equinor/spine-splinter/issues/49)) ([125f8b5](https://github.com/equinor/spine-splinter/commit/125f8b5192a768d4642afcd3da0279913d07e19f))
* Configure sku per env ([#37](https://github.com/equinor/spine-splinter/issues/37)) ([8987a01](https://github.com/equinor/spine-splinter/commit/8987a01be403fd6712b2d8c780fe8f6684d133d2))
* setup endpoints to fuseki ([#23](https://github.com/equinor/spine-splinter/issues/23)) ([118f4d7](https://github.com/equinor/spine-splinter/commit/118f4d7fa795cfa98ff252b903c38bfc9b79c631))
* Setup error-handling for Fuseki ([#18](https://github.com/equinor/spine-splinter/issues/18)) ([a139f80](https://github.com/equinor/spine-splinter/commit/a139f80bcc215070ec932a55525833e1a0c44d29))
* Setup initial roles ([#26](https://github.com/equinor/spine-splinter/issues/26)) ([8a99d8e](https://github.com/equinor/spine-splinter/commit/8a99d8e2e172005848ae994774c30592c600b0a6))


### Bug Fixes

* 🐛 Add AzFunction to AccessPolicy for KeyVault ([#34](https://github.com/equinor/spine-splinter/issues/34)) ([858cc6a](https://github.com/equinor/spine-splinter/commit/858cc6ac49579e5418eec5110eb3c9159cfccae0))
* 🐛 Remove comma in bicep ([8a58cae](https://github.com/equinor/spine-splinter/commit/8a58caed2583907fa8fac8bef704a49e3d92a10e))
* 🐛 Use MicrosoftIdentityWebApiAuthentication directly ([#33](https://github.com/equinor/spine-splinter/issues/33)) ([65979c1](https://github.com/equinor/spine-splinter/commit/65979c10cfcefeaa94c5b4faa6ebe01bfc82616c))
* 74490 improve shipweight queries ([#51](https://github.com/equinor/spine-splinter/issues/51)) ([25544f9](https://github.com/equinor/spine-splinter/commit/25544f919582494d33d06b0694f423f401526f35))
* Added default logging config ([8ce7088](https://github.com/equinor/spine-splinter/commit/8ce7088ebca3f6bb4f7318afa1b79dcdc4f3dc89))
* fix missing bicep [#63](https://github.com/equinor/spine-splinter/issues/63) ([ac33b0e](https://github.com/equinor/spine-splinter/commit/ac33b0e9554ad183d327c5705e9f24c4a60b68e3))
* fixed issue retrieving revision from Fuseki ([#42](https://github.com/equinor/spine-splinter/issues/42)) ([d9eec84](https://github.com/equinor/spine-splinter/commit/d9eec84e560a48a8bde8fd43d00684776cccb0b4))
* Update Sku to p1v2 ([#44](https://github.com/equinor/spine-splinter/issues/44)) ([2cf0b05](https://github.com/equinor/spine-splinter/commit/2cf0b057d9285ba6d7fb292e9ece74a72b675a58))


### Other

* 💡 use downstream api against fusekis ([#30](https://github.com/equinor/spine-splinter/issues/30)) ([d30faf7](https://github.com/equinor/spine-splinter/commit/d30faf70c696b1203ca6f91f149eb176b2af73b2))
* 🤖 add key vault and auth ([#2](https://github.com/equinor/spine-splinter/issues/2)) ([599bdc9](https://github.com/equinor/spine-splinter/commit/599bdc952078cd4f96044ffb0601d30f234cde77))
* 🤖 Add tags for team, env, product  on resources ([b6861cb](https://github.com/equinor/spine-splinter/commit/b6861cb3faf803e4e375af8ac9195a0662721f3c))
* 🤖 remove unused code ([#17](https://github.com/equinor/spine-splinter/issues/17)) ([5e100c9](https://github.com/equinor/spine-splinter/commit/5e100c9e6a2ce65158abdc37e617bc73025d4ae8))
* 71060/migrate excel2rdf and shipweight cli ([#28](https://github.com/equinor/spine-splinter/issues/28)) ([59bf476](https://github.com/equinor/spine-splinter/commit/59bf476f9ea0fd2db492a751d76902026909bf64))
* 71137 splinter provenance generation ([#36](https://github.com/equinor/spine-splinter/issues/36)) ([3d65a9c](https://github.com/equinor/spine-splinter/commit/3d65a9c0010a7243397a0104276b8eec8a7b373b))
* 71181/move ship weight db logic to cli ([#29](https://github.com/equinor/spine-splinter/issues/29)) ([26f9acc](https://github.com/equinor/spine-splinter/commit/26f9accc58319043923e73b43e9931ba8e97e035))
* 72976 update rdf transformation for tie ([#38](https://github.com/equinor/spine-splinter/issues/38)) ([75b4f81](https://github.com/equinor/spine-splinter/commit/75b4f81dcf35f45d5c05bdae78358e69d4be98a4))
* 73816 push transformed data to default fuseki ([#41](https://github.com/equinor/spine-splinter/issues/41)) ([d0bc6d0](https://github.com/equinor/spine-splinter/commit/d0bc6d0a814334842ab8af7e3fb9536157601dec))
* allow no subscription set in deploy ([d5453b8](https://github.com/equinor/spine-splinter/commit/d5453b83adf2a8924da9683e77c4c63f70c5cbfc))
* **deps:** bump actions/setup-dotnet from 2.0.0 to 2.1.0 ([#7](https://github.com/equinor/spine-splinter/issues/7)) ([6f1f7a3](https://github.com/equinor/spine-splinter/commit/6f1f7a300889cd981b917092268ac9aea0816472))
* **deps:** bump Azure.Messaging.ServiceBus from 7.8.1 to 7.9.0 ([#54](https://github.com/equinor/spine-splinter/issues/54)) ([1411504](https://github.com/equinor/spine-splinter/commit/141150487c6cd8dc300021b7a808d1b2beb9fb18))
* **deps:** bump Azure.Storage.Blobs from 12.12.0 to 12.13.0 ([#53](https://github.com/equinor/spine-splinter/issues/53)) ([0f0ad46](https://github.com/equinor/spine-splinter/commit/0f0ad4693b4be1fccd174271cf9ce23cf4db5634))
* **deps:** bump Azure/login from 1.4.4 to 1.4.5 ([#50](https://github.com/equinor/spine-splinter/issues/50)) ([40001b5](https://github.com/equinor/spine-splinter/commit/40001b539ccd5afb64bd5d64133adb897b7c9c22))
* **deps:** bump coverlet.collector from 3.0.2 to 3.1.2 ([#15](https://github.com/equinor/spine-splinter/issues/15)) ([59f32be](https://github.com/equinor/spine-splinter/commit/59f32bed64a6e16c2f461ead66b7050a1bbc079b))
* **deps:** bump DocumentFormat.OpenXml from 2.15.0 to 2.16.0 ([#12](https://github.com/equinor/spine-splinter/issues/12)) ([acd2af5](https://github.com/equinor/spine-splinter/commit/acd2af578204f14de9150696719a629e99b4fd38))
* **deps:** bump DocumentFormat.OpenXml from 2.16.0 to 2.17.1 ([#52](https://github.com/equinor/spine-splinter/issues/52)) ([3cc6db7](https://github.com/equinor/spine-splinter/commit/3cc6db75c0d5cc027da74ec2f49f9d35cbc4f592))
* **deps:** bump dotnetRDF from 2.7.0 to 2.7.4 ([#14](https://github.com/equinor/spine-splinter/issues/14)) ([352d9ad](https://github.com/equinor/spine-splinter/commit/352d9ad2792cbbc2236e56808428f2ea399b294c))
* **deps:** bump dotnetRDF from 2.7.4 to 2.7.5 ([#59](https://github.com/equinor/spine-splinter/issues/59)) ([e770f6b](https://github.com/equinor/spine-splinter/commit/e770f6bb259f2ff757cfa727adfce598190378b8))
* **deps:** bump Microsoft.AspNetCore.Authentication.JwtBearer ([#22](https://github.com/equinor/spine-splinter/issues/22)) ([e596caa](https://github.com/equinor/spine-splinter/commit/e596caa8b48eb5fd968aff9d400d9aa8fdedd781))
* **deps:** bump Microsoft.AspNetCore.Authentication.JwtBearer ([#39](https://github.com/equinor/spine-splinter/issues/39)) ([c1aaf26](https://github.com/equinor/spine-splinter/commit/c1aaf26245a84c210d0807ad7b8e477efb0572dc))
* **deps:** bump Microsoft.AspNetCore.Authentication.JwtBearer ([#55](https://github.com/equinor/spine-splinter/issues/55)) ([4c9db57](https://github.com/equinor/spine-splinter/commit/4c9db57b9dff9e4c2f20e6c506ee30c09d2613ec))
* **deps:** bump Microsoft.AspNetCore.Authentication.JwtBearer ([#9](https://github.com/equinor/spine-splinter/issues/9)) ([535ee60](https://github.com/equinor/spine-splinter/commit/535ee60df84696db859d4702a4446a48dabf3d7b))
* **deps:** bump Microsoft.AspNetCore.Authentication.OpenIdConnect ([#10](https://github.com/equinor/spine-splinter/issues/10)) ([2a7c61d](https://github.com/equinor/spine-splinter/commit/2a7c61db553ee651f687a69ae3c8804e33faa907))
* **deps:** bump Microsoft.AspNetCore.Authentication.OpenIdConnect ([#21](https://github.com/equinor/spine-splinter/issues/21)) ([3bfa5e5](https://github.com/equinor/spine-splinter/commit/3bfa5e536faf7c183e56777d10ecf7136adb5b56))
* **deps:** bump Microsoft.AspNetCore.Authentication.OpenIdConnect ([#40](https://github.com/equinor/spine-splinter/issues/40)) ([3477e58](https://github.com/equinor/spine-splinter/commit/3477e58a9b3c5bf5b149680ff1b6aac53bf435f2))
* **deps:** bump Microsoft.AspNetCore.Authentication.OpenIdConnect ([#56](https://github.com/equinor/spine-splinter/issues/56)) ([818ba13](https://github.com/equinor/spine-splinter/commit/818ba13bd9dad4e1ff308dba9bb203926bb634f7))
* **deps:** bump microsoft.data.sqlclient from 4.0.1 to 4.1.0 ([#16](https://github.com/equinor/spine-splinter/issues/16)) ([491ca7a](https://github.com/equinor/spine-splinter/commit/491ca7a5ca302bc68dd8f803d0ec9279f9d54207))
* **deps:** bump Microsoft.Extensions.Azure from 1.1.1 to 1.2.0 ([#19](https://github.com/equinor/spine-splinter/issues/19)) ([454db10](https://github.com/equinor/spine-splinter/commit/454db1091a86643aa9f6d0b1a49869800da09a1e))
* **deps:** bump Microsoft.Extensions.Azure from 1.2.0 to 1.3.0 ([#57](https://github.com/equinor/spine-splinter/issues/57)) ([73ede2e](https://github.com/equinor/spine-splinter/commit/73ede2e47b774dbeaa162206d3f22fad5bc5d414))
* **deps:** bump Microsoft.Identity.Web from 1.16.0 to 1.24.1 ([#8](https://github.com/equinor/spine-splinter/issues/8)) ([b677332](https://github.com/equinor/spine-splinter/commit/b677332370cae03a24789035ab3e9d576dbddfe4))
* **deps:** bump Microsoft.Identity.Web from 1.24.1 to 1.25.0 ([#35](https://github.com/equinor/spine-splinter/issues/35)) ([c51f74e](https://github.com/equinor/spine-splinter/commit/c51f74e186fb8094e338e242753ac349f39eec1d))
* **deps:** bump Microsoft.Identity.Web from 1.25.0 to 1.25.1 ([#48](https://github.com/equinor/spine-splinter/issues/48)) ([8ca927b](https://github.com/equinor/spine-splinter/commit/8ca927b55b38b9ddc2fc8820604caa185e773966))
* **deps:** bump Microsoft.NET.Test.Sdk from 16.9.4 to 17.1.0 ([#20](https://github.com/equinor/spine-splinter/issues/20)) ([30f924b](https://github.com/equinor/spine-splinter/commit/30f924bc1006d39bc5f38844a06e65c86640f355))
* **deps:** bump Microsoft.NET.Test.Sdk from 17.1.0 to 17.2.0 ([#25](https://github.com/equinor/spine-splinter/issues/25)) ([13d8ed6](https://github.com/equinor/spine-splinter/commit/13d8ed6b4cf9eac0f0b9cf0a603d579fd4224ef8))
* **deps:** bump Serilog.AspNetCore from 5.0.0 to 6.0.0 ([#60](https://github.com/equinor/spine-splinter/issues/60)) ([57428d4](https://github.com/equinor/spine-splinter/commit/57428d49dcbe70f68831f2b2e4d13b8d6787218a))
* **deps:** bump Serilog.Sinks.ApplicationInsights from 3.1.0 to 4.0.0 ([#47](https://github.com/equinor/spine-splinter/issues/47)) ([81fa785](https://github.com/equinor/spine-splinter/commit/81fa78529dccab48692e493b7872cd8b6d838125))
* **deps:** bump Swashbuckle.AspNetCore from 6.2.3 to 6.3.1 ([#6](https://github.com/equinor/spine-splinter/issues/6)) ([1210e05](https://github.com/equinor/spine-splinter/commit/1210e05e96925e7df91f3ebe23036750e3f0ff4b))
* **deps:** bump Swashbuckle.AspNetCore from 6.3.1 to 6.3.2 ([#58](https://github.com/equinor/spine-splinter/issues/58)) ([e27e5ae](https://github.com/equinor/spine-splinter/commit/e27e5aeb040f06dd06fab9312c10a0dad0dc5234))
* **deps:** bump xunit.runner.visualstudio from 2.4.3 to 2.4.5 ([#13](https://github.com/equinor/spine-splinter/issues/13)) ([04b36d9](https://github.com/equinor/spine-splinter/commit/04b36d91b2886e6dbd69b8c0d43a3438ccd96629))
* fix appsettings.json ([a9fc23e](https://github.com/equinor/spine-splinter/commit/a9fc23edb15ef3ad5216d51f148744c7c07eb6dc))
* Migrated latest version of Doc2Rdf from dugtrio-experimental ([#27](https://github.com/equinor/spine-splinter/issues/27)) ([b67e1b9](https://github.com/equinor/spine-splinter/commit/b67e1b91c2a85e673f6e8941de0b70453d2afe4d))
* Removed debug from application insight ([8234ad1](https://github.com/equinor/spine-splinter/commit/8234ad11fdf852e57ad82f7b2a90ee2d8d3a8b58))
* turn off launch browser ([03777dd](https://github.com/equinor/spine-splinter/commit/03777ddaef03c31a92c800ef85b06f47d41183cb))


### CI/CD

* 🎡 add bicep ([#1](https://github.com/equinor/spine-splinter/issues/1)) ([2b4c795](https://github.com/equinor/spine-splinter/commit/2b4c795026c5e27dbc355572f0766668e0063ba2))
* 🎡 Add CI/CD Github Actions ([#5](https://github.com/equinor/spine-splinter/issues/5)) ([5c467a0](https://github.com/equinor/spine-splinter/commit/5c467a05bd7dfae90b634ffe0c6bbd0b0b014469))
* 🎡 Disable func alwayson ([e2e5a47](https://github.com/equinor/spine-splinter/commit/e2e5a4784e40bf41004bee9f4320297e2a8ede17))
* 🎡 Setup consumption-plan for function ([#45](https://github.com/equinor/spine-splinter/issues/45)) ([6e85dbd](https://github.com/equinor/spine-splinter/commit/6e85dbd3a34a085d97a2828010202531db16a8fe))
* 🎡 setup dynamic plan ([9d0251c](https://github.com/equinor/spine-splinter/commit/9d0251c8f2c45213435e8fefe45640f4e0379cd2))
* 🎡 Verify IaC with what-if ([#46](https://github.com/equinor/spine-splinter/issues/46)) ([6f6b93b](https://github.com/equinor/spine-splinter/commit/6f6b93b6e673d97978a32092820a8a964e8f493f))
* make ready for prod ([#62](https://github.com/equinor/spine-splinter/issues/62)) ([b1ba81d](https://github.com/equinor/spine-splinter/commit/b1ba81d4f864161fd5fcc7e29ef643c75cabc72b))
* No longer neccessary env set in cq ([244ea3e](https://github.com/equinor/spine-splinter/commit/244ea3ee775d71859ffb899cc0780aae131214a9))
