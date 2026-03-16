# About The Project
This project is a configurable simulation framework for modelling how independent agents react to a dynamic environment using a combination of cellular automata (CA) for environment evolution and state machines (SM) for agent behaviour. It targets scenarios such as building evacuations, disease spread, plant growth, and other systems that can be represented as agents moving within time-varying environments.

## Features
* Dynamic multi-layer environment evolved using cellular automata with GPU-accelerated convolution.
* Modular agents driven by finite state machines with pluggable inputs, states, and transitions.
* Navigation graphs per layer with environment-dependent edge weights for dynamic pathfinding.
* Multi-layer (“3D”) simulations with layer interaction via bleed points and walls via stop values.
* Simple 2D UI for starting, pausing, saving, loading, zooming and panning simulations.
* Rust CLI tool to convert PNG images into environment layers and navigation graphs.

## Architecture
The system is divided into four subsystems:
* CASM Editor: user interaction for loading/saving simulations, controlling play/pause and editing.
* Environment: Manages stacked 2D layers, their Convolution based evolution and associated navigation graphs.
* Agent: Manages agent state, uses shared SMs plus per-agent sensors to advance behaviour and movement.
* Gearbox: Mediates interactions between agents and environment (cell reads/writes, pathfinding requests and coordinate conversions).
<img width="1091" height="715" alt="image" src="https://github.com/user-attachments/assets/a36f7496-48f5-4547-952d-2f9c7efb74bd" />

# Getting started 
Clone this repo and launch using unity. This projcet comes with a few example simulations in the following dirs
* In layers/examples simulations designed so that you can look at the raw files being loaded.
* In layers/experiments dir are the experiments that were performed for and discussed in my Dissertation.

# Dissertation
https://docs.google.com/document/d/13Ht469jD2GUlGsKPA9Ll0Lu0RdNR873xCY7zyj1wK3w/edit?usp=sharing
