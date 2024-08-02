
# Unity Netcode/Networking Solution Benchmark

This repository holds the various netcode projects and It's benchmark result such as:
- Bandwidth
- CPU Server Usage

### Benchmark Purpose

- To share insights which netcode is the most performant by **DEFAULT**
- Check benchmark from their official claims
- To let the benchmark to be customized & used for everybody. **Not for Speculation**

### Benchmark Info

- This benchmark doesn’t use the best settings for each Netcode
- This is made to remove the bias netcode **EXCEPT** the netcode enabled that “feature” by default
    - Example
        - Tweaking Fusion's compression accuracy
        - Fishnet doesn’t use network LOD

### Netcode List
- Fusion 1 & 2
- Netick 2
- Fishnet
- Mirror
- NGO (Netcode for GameObject)
- Mirage

## Project Config

### Unity Editor

- Mono
- Render Pipeline: BiRP
- Windows
- Unity 2021.3.21f1

## Bandwidth Measuring

- It is highly recommended to not use built-in net stats, they may be inaccurate due to transport & protocol overhead.
- Wireshark is one of the software to analyze network traffics, it can be used with filter of: `udp.srcport == {serverport}`

# Benchmark Result

## Bandwidth
- [07/12/23 - LATEST](benchmark-result/bandwidth/07-12-2023.md)

## Server CPU Usage
- [17/12/2024 - WIP](benchmark-result/server-cpu/17-03-2024.md)

# Roadmap

## Planned
- IL2CPP Server CPU Testing
- More methods of Server CPU Testing

## Nice to Have
- Memory Usage Benchmark

# FAQ

## How to Test by my Own?

### Fusion

- Developers have to import the fusion package by their own
    - Fusion has ToS to not put their code online (public repo)
- Create your own Realtime app settings

### Other Netcode

- Just build the Game!

## Can I Want to Contribute

- Yes, It is open for communities
- You can customized your benchmark and create an Issue on github, I’ll test on my own and verify that. If it clears, I’ll proceed to add that to custom user benchmark

## Results is Misleading

- If so, we can compare our proofing, if current benchmark is wrong, I’ll retest and update it
    - This is only **VALID** if you have the same configuration as the benchmark

## When to expect for benchmark update?

- Communities are Offered to test their own, upgrade the sdk, create issues on github repo
    - Then, It will be merged to the master repo

## Why Don’t you enable 'insert feature name'?

- This will endlessly happens, some netcode doesn’t have the feature equivalent
- The time I enable 'insert feature name', other netcode bias will also tell me to "enable this...",  "do this..."
- Example Case
    - Enable compression in Netcode X
    - Another netcode bias tell to enable this too on Netcode Y
    - Enabled that feature on Netcode Y
    - Another bias netcode tell to enable another one on Netcode Z
    - Endless…

## Commit Message Guideline

- Because this repo contains multiple projects, It is recommended to create your commit message to be
    - `<projectName> <context>: <commit message>`
    - Example:
        - `fusion feat: add network transform to player prefab`
