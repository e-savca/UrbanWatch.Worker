pipeline {
    agent any

    triggers {
        pollSCM 'H * * * *'
    }

    environment {
        IMAGE_NAME = 'urbanwatch-worker'
        IMAGE_TAG = 'latest'
    }

    stages {
        stage('Docker Build') {
            steps {
                script {
                    sh "docker build -t ${IMAGE_NAME}:${IMAGE_TAG} ."
                }
            }
        }

        stage('Docker Push') {
            steps {
                withCredentials([
                    usernamePassword(
                        credentialsId: 'github-personal-account',
                        usernameVariable: 'GHCR_USER',
                        passwordVariable: 'GHCR_PASS'
                    )
                ]) {
                    script {
                        def ghcrImage = "ghcr.io/${GHCR_USER}/${IMAGE_NAME}:${IMAGE_TAG}"

                        sh """
                            echo "$GHCR_PASS" | docker login ghcr.io -u "$GHCR_USER" --password-stdin
                            docker tag ${IMAGE_NAME}:${IMAGE_TAG} ${ghcrImage}
                            docker push ${ghcrImage}
                        """
                    }
                }
            }
        }
    }

    post {
        always {
            script {
                sh 'docker images'
            }
            cleanWs()
        }
    }
}
