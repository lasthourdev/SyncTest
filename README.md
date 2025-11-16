# SyncDash

## Overview
A hyper-casual endless runner featuring:
- Right side: Player-controlled cube
- Left side: Ghost cube synced in real-time

## Gameplay
- ◄ ► Swipe to switch lanes (3 lanes)
- ▲ Swipe up to jump
- 🟩 Collect green cubes to score
- 🟥 Avoid red cubes to keep lives

## Features
- 🔄 Real-time local sync of player state & spawns
- 📦 Object pooling for obstacles & collectibles
- 🎲 Pattern-based spawn system
- 📊 Basic UI with menus, score & lives display
- ✨ Particle effects on collisions & collections

## Key Scripts

| Script          | Role                                              |
|-----------------|---------------------------------------------------|
| GameManager     | Manages game states, score, lives & UI flow        |
| SyncManager    | Buffers & applies player states for ghost sync      |
| PoolManager    | Object pooling for obstacles & orbs                  |
| SpawnManager   | Spawns obstacles & orbs per patterns                  |
| PlayerController | Handles input, movement, jumping, collisions & broadcasting |
| Obstacle       | Controls obstacle behavior & lifecycle                |
| UIManager      | Controls UI panels & updates score/lives text        |
