## [1.12.2-beta.4](https://github.com/PurrNet/PurrNet/compare/v1.12.2-beta.3...v1.12.2-beta.4) (2025-06-26)


### Bug Fixes

* composite transport ([4c84b41](https://github.com/PurrNet/PurrNet/commit/4c84b41640a817a6e01f4ba72d8d18af252dec03))

## [1.12.2-beta.3](https://github.com/PurrNet/PurrNet/compare/v1.12.2-beta.2...v1.12.2-beta.3) (2025-06-26)


### Bug Fixes

* proper comparer ([a30043c](https://github.com/PurrNet/PurrNet/commit/a30043c802391a2b98ad65502e93d1012f7edef8))

## [1.12.2-beta.2](https://github.com/PurrNet/PurrNet/compare/v1.12.2-beta.1...v1.12.2-beta.2) (2025-06-26)


### Bug Fixes

* boost IL processing performance ([7d32309](https://github.com/PurrNet/PurrNet/commit/7d32309df8c4f0cbf2951d806528df25ddde2c8e))

## [1.12.2-beta.1](https://github.com/PurrNet/PurrNet/compare/v1.12.1...v1.12.2-beta.1) (2025-06-26)


### Bug Fixes

* do ownership stuff on early observer added ([e5724c6](https://github.com/PurrNet/PurrNet/commit/e5724c6d37a8c5dab40f6fe5cd21c7570deaa8c1))

## [1.12.1](https://github.com/PurrNet/PurrNet/compare/v1.12.0...v1.12.1) (2025-06-25)


### Bug Fixes

* check if networkAssets isnt null ([1038e1a](https://github.com/PurrNet/PurrNet/commit/1038e1a1e90af75a4b6de4bdac8888fdda06f2f5))

## [1.12.1-beta.1](https://github.com/PurrNet/PurrNet/compare/v1.12.0...v1.12.1-beta.1) (2025-06-25)


### Bug Fixes

* check if networkAssets isnt null ([1038e1a](https://github.com/PurrNet/PurrNet/commit/1038e1a1e90af75a4b6de4bdac8888fdda06f2f5))

# [1.12.0](https://github.com/PurrNet/PurrNet/compare/v1.11.1...v1.12.0) (2025-06-25)


### Bug Fixes

* `GetSpawnedParent` can throw an error ([513ce28](https://github.com/PurrNet/PurrNet/commit/513ce2845c0bcc6ec06ed9ed9574219e32d58d41))
* actually call Optimize on network animator batch ([a53f8e3](https://github.com/PurrNet/PurrNet/commit/a53f8e327ea65cceb56ff09e0a884cda6c152a2c))
* add `ServerOnlyAttribute` ([747451a](https://github.com/PurrNet/PurrNet/commit/747451a54e121107f3a87f2d22238a9eca255e87))
* add a `AlwaysIncludeDontDestroyOnLoadScene` in the network rules ([9404b77](https://github.com/PurrNet/PurrNet/commit/9404b778a7fbbe5a18201886da52f4c7f3524be6))
* add onPreProcessRpc and onPostProcessRpc to the RPCModule ([c588685](https://github.com/PurrNet/PurrNet/commit/c5886856b03a774452fd0618e480baefa2bb0655))
* added a changelog ([13af73d](https://github.com/PurrNet/PurrNet/commit/13af73dceddb751b26a8d25f37d485fe79706a25))
* Allow to save bandwidth to file and then load it in the editor ([2117e33](https://github.com/PurrNet/PurrNet/commit/2117e3355b268ef455f5c56cc13d05612f33098c))
* always gen the rpc signature ([1afa09c](https://github.com/PurrNet/PurrNet/commit/1afa09c6e0c45971251da0f9a395bd281ed0074c))
* batch acks for delta module ([cc4c89d](https://github.com/PurrNet/PurrNet/commit/cc4c89dbe46d2c29e717a52d4968a0226dd5cfa5))
* better error for when sync modules miss permissions ([e28df7b](https://github.com/PurrNet/PurrNet/commit/e28df7b9587fcdf47a7ae799f0c4bb9bcda16920))
* better static generic type discovery ([da5f6e9](https://github.com/PurrNet/PurrNet/commit/da5f6e954ed4727c6f09034ab8291c0036f95a93))
* better visibility API ([3af2c32](https://github.com/PurrNet/PurrNet/commit/3af2c32f62426564feb14db552412c66ed8bfd84))
* BitPacker being in Write mode when received for Reading ([7ebb8aa](https://github.com/PurrNet/PurrNet/commit/7ebb8aa45a3fb6f3283f977da42ca44100f84c9f))
* change name of package for openupm ([b759197](https://github.com/PurrNet/PurrNet/commit/b759197c0a11986a029e7caf333d3fe44655e5da))
* copy managed types when calling RPCs locally ([28b7091](https://github.com/PurrNet/PurrNet/commit/28b70917a70429f84332b1acefcc82fedf6bf272))
* DontPackAttribute only works for field ([5846ecd](https://github.com/PurrNet/PurrNet/commit/5846ecd9a5c4f2d9a07e41361f64e67ac8ddb0ec))
* ensure that it at least replaces with empty method for `ServerOnly` ([9750c5d](https://github.com/PurrNet/PurrNet/commit/9750c5d620e05c10421c0f0578451285d58358eb))
* enum delta packers weren't implemented ([13ed11f](https://github.com/PurrNet/PurrNet/commit/13ed11f922651136ee52b3e7ab09a91c7ca52902))
* Expanded the rtt summary ([7668055](https://github.com/PurrNet/PurrNet/commit/766805521bacdba984a15deb9f8011aed71c78c5))
* if server, always use the ownerServer value ([9626f51](https://github.com/PurrNet/PurrNet/commit/9626f513957ec5db316e27807bc622786820879e))
* improved statistics manager ([8fed412](https://github.com/PurrNet/PurrNet/commit/8fed412172ffdb88d74d7b80c1d093052f10644c))
* include full type for generic too ([4990d69](https://github.com/PurrNet/PurrNet/commit/4990d6983b059c20252c9dafd80250c6b93824e0))
* introduced DontPack attribute ([2fea79e](https://github.com/PurrNet/PurrNet/commit/2fea79e8cc8e2598001e29ab73b51fe4feaf7eb9))
* LastNID patch, this needs to be reworked ([16dc6d3](https://github.com/PurrNet/PurrNet/commit/16dc6d30cec6c85eb8fad123be0a3bfee2299a5a))
* link the changlog ([9ef043a](https://github.com/PurrNet/PurrNet/commit/9ef043a70732867218d4aaf98f0d2e7c0c38fbf0))
* make core unity dependencies optional ([12b06e1](https://github.com/PurrNet/PurrNet/commit/12b06e191792bb7d1c7416621c2c500af044f935))
* metadata file for CHANGELOG.md ([dd139fc](https://github.com/PurrNet/PurrNet/commit/dd139fc066987c8942d8751d6f194a917fa9616c))
* missing using ([0f51df2](https://github.com/PurrNet/PurrNet/commit/0f51df2921e55dc28c483d4efe444267dc14fab5))
* Network assets pull multiple sub assets ([de49d8b](https://github.com/PurrNet/PurrNet/commit/de49d8b07fdabb9057336bfef4317c806e7d6357))
* Network Assets working with Sub-assets ([769ff32](https://github.com/PurrNet/PurrNet/commit/769ff32e111da0315d6c077c0e1c8e41902a8900))
* network reflection and network assets ([1adea71](https://github.com/PurrNet/PurrNet/commit/1adea71cf4a1517122a5130429500a4a99ece8fa))
* only keep latest `SetX` for animation ([badec0d](https://github.com/PurrNet/PurrNet/commit/badec0dd5b6f56b88085f4e1ea6195ff4a3d33cf))
* ownership events ([9a245f9](https://github.com/PurrNet/PurrNet/commit/9a245f9c7dd4a9a70da9daa2fd27c57db84b711f))
* properly populate RPCInfo for runlocally ([bd99145](https://github.com/PurrNet/PurrNet/commit/bd991450479f1b09bff4e2be463e9cfd8c9b567a))
* refactoring `AreEqual` helpers for the packer ([20b2c70](https://github.com/PurrNet/PurrNet/commit/20b2c70665be9960e6df05776ebe261e53a45c7b))
* remove UniTask as a dependency ([725cabf](https://github.com/PurrNet/PurrNet/commit/725cabfc54a037375e94fb16ccbcb2e1d94aead7))
* reverted bad changes ([94914f4](https://github.com/PurrNet/PurrNet/commit/94914f4b907105abf1f4646551d61210c706eff4))
* server rpc's on server should not use the network ([06b6d9d](https://github.com/PurrNet/PurrNet/commit/06b6d9d15a78c7b908367af60ffea1e1137b9115))
* set target frame rate to tick rate for server builds ([b1fc358](https://github.com/PurrNet/PurrNet/commit/b1fc35896b66e2ea69f13910962e1a82199787c7))
* start server/client, stop server/client always calls the network manager and does it through it instead of individually, otherwise things are unpredictable ([157d47c](https://github.com/PurrNet/PurrNet/commit/157d47cd8405893fd0180b9621f58fc3e6da788b))
* state machine editor issues in prefab runtime ([d0ad04a](https://github.com/PurrNet/PurrNet/commit/d0ad04a033fe5e0d860cdd11a6d1cd9be8a16c46))
* State machine exit on despawn ([9884c58](https://github.com/PurrNet/PurrNet/commit/9884c585b1aa8950b56fbc7db82d58d1039bc864))
* Statistics manager improvements ([f494ce9](https://github.com/PurrNet/PurrNet/commit/f494ce96b947ea8a69d049ed50adc39ab4432ac6))
* Statistics manager jitter ([0c5d611](https://github.com/PurrNet/PurrNet/commit/0c5d611b215a5d049c3494c58c189b3b5c4ff8b9))
* steam server not properly cleaning internal state ([af3a793](https://github.com/PurrNet/PurrNet/commit/af3a7932271bf7547e8d14bfc23a26e539aa3445))
* Sync dictionary sending for clients ([88ce60a](https://github.com/PurrNet/PurrNet/commit/88ce60a2f56e5d594a9f2c54b055eaef8790d4b9))
* Sync types for strict rules ([7722477](https://github.com/PurrNet/PurrNet/commit/7722477cba75fc22b49c6b23af70d4e4b5d57132))
* undo mess ([9f0f26c](https://github.com/PurrNet/PurrNet/commit/9f0f26c336b16ec78d6f340dd529286cf5c05fad))
* unityProxyType being null caused IL issues ([15a85cd](https://github.com/PurrNet/PurrNet/commit/15a85cd3b10ec0865965ad5fa190a68467879f3c))
* weird ownership order ([634ed88](https://github.com/PurrNet/PurrNet/commit/634ed88a8098049f9455cda503b0f5eb7cf7a96e))
* when sending a target rpc to local player just call it locally ([2982811](https://github.com/PurrNet/PurrNet/commit/2982811a01626b4f0cdf0da0378c5c25a26aa2ff))


### Features

* Network assets added ([16ebe3c](https://github.com/PurrNet/PurrNet/commit/16ebe3c4e91db8ab14f0d7c075294bae0354f33c))
* unity editor toolbar with purrnet state ([dbdb6cb](https://github.com/PurrNet/PurrNet/commit/dbdb6cb04ac88fb364826430c2a32273ad8e79b8))

# [1.12.0-beta.11](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.10...v1.12.0-beta.11) (2025-06-25)


### Bug Fixes

* BitPacker being in Write mode when received for Reading ([7ebb8aa](https://github.com/PurrNet/PurrNet/commit/7ebb8aa45a3fb6f3283f977da42ca44100f84c9f))
* network reflection and network assets ([1adea71](https://github.com/PurrNet/PurrNet/commit/1adea71cf4a1517122a5130429500a4a99ece8fa))

# [1.12.0-beta.10](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.9...v1.12.0-beta.10) (2025-06-24)


### Bug Fixes

* set target frame rate to tick rate for server builds ([b1fc358](https://github.com/PurrNet/PurrNet/commit/b1fc35896b66e2ea69f13910962e1a82199787c7))

# [1.12.0-beta.9](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.8...v1.12.0-beta.9) (2025-06-24)


### Bug Fixes

* Network assets pull multiple sub assets ([de49d8b](https://github.com/PurrNet/PurrNet/commit/de49d8b07fdabb9057336bfef4317c806e7d6357))

# [1.12.0-beta.8](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.7...v1.12.0-beta.8) (2025-06-24)


### Bug Fixes

* Network Assets working with Sub-assets ([769ff32](https://github.com/PurrNet/PurrNet/commit/769ff32e111da0315d6c077c0e1c8e41902a8900))

# [1.12.0-beta.7](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.6...v1.12.0-beta.7) (2025-06-24)


### Bug Fixes

* ownership events ([9a245f9](https://github.com/PurrNet/PurrNet/commit/9a245f9c7dd4a9a70da9daa2fd27c57db84b711f))

# [1.12.0-beta.6](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.5...v1.12.0-beta.6) (2025-06-23)


### Bug Fixes

* Statistics manager jitter ([0c5d611](https://github.com/PurrNet/PurrNet/commit/0c5d611b215a5d049c3494c58c189b3b5c4ff8b9))

# [1.12.0-beta.5](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.4...v1.12.0-beta.5) (2025-06-22)


### Bug Fixes

* include full type for generic too ([4990d69](https://github.com/PurrNet/PurrNet/commit/4990d6983b059c20252c9dafd80250c6b93824e0))

# [1.12.0-beta.4](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.3...v1.12.0-beta.4) (2025-06-22)


### Bug Fixes

* better static generic type discovery ([da5f6e9](https://github.com/PurrNet/PurrNet/commit/da5f6e954ed4727c6f09034ab8291c0036f95a93))

# [1.12.0-beta.3](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.2...v1.12.0-beta.3) (2025-06-22)


### Bug Fixes

* Expanded the rtt summary ([7668055](https://github.com/PurrNet/PurrNet/commit/766805521bacdba984a15deb9f8011aed71c78c5))

# [1.12.0-beta.2](https://github.com/PurrNet/PurrNet/compare/v1.12.0-beta.1...v1.12.0-beta.2) (2025-06-22)


### Features

* unity editor toolbar with purrnet state ([dbdb6cb](https://github.com/PurrNet/PurrNet/commit/dbdb6cb04ac88fb364826430c2a32273ad8e79b8))

# [1.12.0-beta.1](https://github.com/PurrNet/PurrNet/compare/v1.11.2-beta.41...v1.12.0-beta.1) (2025-06-20)


### Features

* Network assets added ([16ebe3c](https://github.com/PurrNet/PurrNet/commit/16ebe3c4e91db8ab14f0d7c075294bae0354f33c))

## [1.11.2-beta.41](https://github.com/PurrNet/PurrNet/compare/v1.11.2-beta.40...v1.11.2-beta.41) (2025-06-20)


### Bug Fixes

* weird ownership order ([634ed88](https://github.com/PurrNet/PurrNet/commit/634ed88a8098049f9455cda503b0f5eb7cf7a96e))

## [1.11.2-beta.40](https://github.com/PurrNet/PurrNet/compare/v1.11.2-beta.39...v1.11.2-beta.40) (2025-06-20)


### Bug Fixes

* link the changlog ([9ef043a](https://github.com/PurrNet/PurrNet/commit/9ef043a70732867218d4aaf98f0d2e7c0c38fbf0))

## [1.11.2-beta.39](https://github.com/PurrNet/PurrNet/compare/v1.11.2-beta.38...v1.11.2-beta.39) (2025-06-20)


### Bug Fixes

* metadata file for CHANGELOG.md ([dd139fc](https://github.com/PurrNet/PurrNet/commit/dd139fc066987c8942d8751d6f194a917fa9616c))

## [1.11.2-beta.38](https://github.com/PurrNet/PurrNet/compare/v1.11.2-beta.37...v1.11.2-beta.38) (2025-06-20)


### Bug Fixes

* added a changelog ([13af73d](https://github.com/PurrNet/PurrNet/commit/13af73dceddb751b26a8d25f37d485fe79706a25))

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

<!-- This section will be automatically populated by semantic-release -->

<!--
## [1.0.0] - YYYY-MM-DD
### Added
- New features

### Changed
- Changes in existing functionality

### Deprecated
- Soon-to-be removed features

### Removed
- Removed features

### Fixed
- Bug fixes

### Security
- Vulnerability fixes
-->
