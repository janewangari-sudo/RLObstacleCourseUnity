# 🧠 RL Obstacle Course Navigator – AgentSphere (Unity ML-Agents)

**AgentSphere** is a reinforcement learning agent trained with Unity ML-Agents to navigate a dynamic 3D obstacle course and reach a moving target. Built as part of a reinforcement learning course, this project showcases curriculum-style training, procedural environments, and robust agent perception using raycasts and vector observations.

📁 **Repo:** [janewangari-sudo/RLObstacleCourseUnity](https://github.com/janewangari-sudo/RLObstacleCourseUnity)  
👩‍💻 **Author:** [Jane Gathongo](https://github.com/janewangari-sudo)

---

## 🎮 Overview

- **Agent**: A white sphere (`AgentSphere`)
- **Goal**: Reach the green target (`TargetSphere`)
- **Environment**:  
  - Procedural static and dynamic obstacles  
  - Moving sinusoidal pillar  
  - Target repositions randomly per success
- **Reward Highlights**:
  - ✅ +1.2 for reaching target  
  - ❌ -0.4 for obstacle/wall collision  
  - 💀 -1.0 for falling off  
  - ⏱️ Per-step and directional penalties/rewards

---

## 🔑 Features

- **3D Procedural Arena** with varying layouts per episode
- **Agent Perception**  
  - `RayPerceptionSensor3D` (9 rays, 120° FoV, 35 units)  
  - 5D vector observations (velocity, target direction, distance)
- **Continuous Control**: 2D force-based movement (X/Z axes)
- **Dynamic Elements**:
  - Sinusoidal obstacle movement (X/Z axis)
  - Randomized spawns for target and obstacles
- **Parallel Training** for faster convergence
- **TensorBoard Monitoring**
- **Extensive Hyperparameter Tuning**

---

## 🛠️ Setup & Training

### Requirements
- Unity `2022.3.6f1`
- Python `3.9`
- `ml-agents==0.30.0`
- PyTorch `1.7.1+cpu`




---

### Training Run Summary
| Color   | Experiment Name                                           | Curiosity | Learning Rate | Hidden Units | Batch Size | Beta | Final Result (Reward) | Observation                                                            |
| ------- | --------------------------------------------------------- | --------- | ------------- | ------------ | ---------- | ---- | --------------------- | ---------------------------------------------------------------------- |
| 🔴 Red  | `Exp_NoCuriosity\AgentSphereBehavior`                     | ❌ No      | 3e-4          | 128          | 1024       | –    | \~1.88                | Very stable, converged to high reward quickly.                         |
| 🔵 Blue | `Exp_With_Curiosity\AgentSphereBehavior`                  | ✅ Yes     | 3e-4          | 128          | 1024       | 0.2  | \~1.76                | Steady improvement, good final reward, moderate episode length.        |
| 🟣 Pink | `Exp_With_Curiosity_Endepisode\AgentSphereBehavior`       | ✅ Yes     | 3e-4          | 128          | 1024       | 0.2  | \~-0.25               | Fluctuations in early stage, eventually stabilizes at moderate reward. |
| ⚪ Gray  | `Exp_With_No_Curiosity_Endepisode\AgentSphereBehavior`    | ❌ No      | 3e-4          | 128          | 1024       | –    | \~-0.25               | Plateaued at negative reward, slow learning progress.                  |
| 🟢 Teal | `Exp_withEndepisodeonHittingObstacle\AgentSphereBehavior` | ❌ No      | 3e-4          | 128          | 1024       | –    | \~0.55                | Good learning, short episodes, consistent positive reward.             |

## 📊 TensorBoard Visualizations (Guideline #8)

The following TensorBoard screenshots illustrate the learning progress of the champion experimental run.

### Champion Training Run: `[YourChampionRunID, e.g., Exp_LongRays_FinalPush]`
*(This graph should clearly show the Mean Cumulative Reward climbing to your best positive value, and the Episode Length decreasing and stabilizing.)*

**[EMBED YOUR BEST TENSORBOARD SCREENSHOT HERE - The one showing the +4.377 (or better) curve for Cumulative Reward and Episode Length. Ensure this image is in your GitHub repo, e.g., in a `docs/images/` folder.]**
*Markdown Example: `![Champion Run: Cumulative Reward & Episode Length](docs/images/ChampionRun_TensorBoard.png)`*

---
## 📷 Screenshots

> Upload images to your repo in an `/assets/` folder and update the links below.

| Agent Navigating | Training Graphs | Arena Setup |
|------------------|-----------------|-------------|
| ![Gameplay](assets/agent-running.gif) | ![Rewards](assets/tensorboard-rewards.png) | ![Arena](assets/arena-layout.png) |

---
## 📺 Watch the Demo

[![Watch the demo](https://img.youtube.com/vi/YOUTUBE_VIDEO_ID/0.jpg)](https://youtu.be/YOUTUBE_VIDEO_ID)

> Replace `YOUTUBE_VIDEO_ID` with your actual YouTube video ID.

---
## 🚀 Getting Started

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

