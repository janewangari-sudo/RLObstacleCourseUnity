#  RL Obstacle Course Navigator – AgentSphere (Unity ML-Agents)

**AgentSphere** is a reinforcement learning agent built with Unity ML-Agents to navigate a dynamic 3D obstacle course and reach a moving target. This project demonstrates curriculum-style training, procedural environments, and robust agent perception using raycasts and vector observations.

---

##  Overview

- **Agent**: White sphere (`AgentSphere`)
- **Goal**: Reach the green `TargetSphere`
- **Environment**: Procedural layout with static/dynamic obstacles, including sinusoidal movement and randomized target positioning
- **Reward System**:
  - +1.2 for reaching the target
  - -0.4 for hitting obstacles/walls
  - -1.0 for falling off
  - Step-wise shaping penalties and rewards

---

##  Features

- Procedural 3D obstacle courses
- Ray-based perception (`RayPerceptionSensor3D`) and vector observations (5D)
- Continuous 2D control using force on X/Z axes
- Randomized spawns and sinusoidal obstacles
- Parallel training for efficiency
- Visualized progress via TensorBoard
- Tuned with various training configurations

---

##  Setup & Training

### Requirements
- Unity `2022.3.6f1`
- Python `3.9`
- `ml-agents==0.30.0`
- PyTorch `1.7.1+cpu`

---
## 📺 Watch the Demo

[![Watch the demo](https://img.youtube.com/vi/YOUTUBE_VIDEO_ID/0.jpg)](https://youtu.be/YOUTUBE_VIDEO_ID)

> Replace `YOUTUBE_VIDEO_ID` with your actual YouTube video ID.

---
##  Getting Started

1.  **Clone the Repository**
    ```bash
    git clone [https://github.com/YOUR_USERNAME/RLObstacleCourseUnity.git](https://github.com/YOUR_USERNAME/RLObstacleCourseUnity.git)
    cd RLObstacleCourseUnity
    ```

2.  **Set Up ML-Agents**
    ```bash
    pip install mlagents
    ```

3.  **Train the Agent**
    ```bash
    mlagents-learn config/config_fast.yaml --run-id=fast_run --train
    ```

4.  **Monitor Training**
    ```bash
    tensorboard --logdir=results
    ```

5.  **Deploy in Unity**
    * Drag your trained `.nn` model into the Unity Editor.
    * Assign it to the Agent’s `Behavior Parameters` component.

## Credits

Created by Jane Gathongo

GitHub: [@janewangari-sudo](https://github.com/janewangari-sudo)

Project repo: [RLObstacleCourseUnity](https://github.com/janewangari-sudo/RLObstacleCourseUnity)

