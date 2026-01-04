pipeline {
    agent any
        
    triggers { githubPush() }
    
    environment {
        SERVER = "${env.DEPLOY_SERVER_hookhub}"
        USER = "${env.DEPLOY_USER_hookhub}"
        DEPLOY_PATH = "${env.DEPLOY_PATH_hookhub}"
        SERVICE = 'HOOKHUB.HUB'
    }
    
    stages {
        stage('Get Code') {
            steps {
                git url: 'https://github.com/mrodriguex/hookhub.git', 
                     credentialsId: 'github-token',
                     branch: 'main'
            }
        }
        
        stage('Build .NET 8') {
            steps {
                sh '''
                    CSPROJ=$(find . -name "HookHub.Hub.csproj" | head -1)
                    dotnet publish "$CSPROJ" -c Release -o ./publish --runtime linux-x64
                '''
            }
        }
        
        stage('Prepare Deployment') {
            steps {
                sshagent(['deployment_key']) {
                    script {
                        // Verificar si el directorio existe remotamente
                        def dirExists = sh(
                            script: """
                                ssh ${USER}@${SERVER} "
                                    if [ -d '${DEPLOY_PATH}' ]; then
                                        echo 'EXISTS'
                                    else
                                        echo 'NOT_EXISTS'
                                    fi
                                "
                            """,
                            returnStdout: true
                        ).trim()
                        
                        if (dirExists == 'NOT_EXISTS') {
                            echo "Creando directorio ${DEPLOY_PATH}..."
                            sh """
                                ssh ${USER}@${SERVER} "
                                    mkdir -p ${DEPLOY_PATH}                                    
                                    echo 'Directorio creado'
                                "
                            """
                        }
                    }
                }
            }
        }

        stage('Deploy') {
            steps {
                sshagent(['deployment_key']) {
                    sh """
                        # Stop service
                        ssh ${USER}@${SERVER} "sudo systemctl stop ${SERVICE}"
                        
                        # Deploy files
                        rsync -avz --delete ./publish/ ${USER}@${SERVER}:${DEPLOY_PATH}/
                        
                        # Restart service
                        ssh ${USER}@${SERVER} "
                            sudo chown -R ${USER}:${USER} ${DEPLOY_PATH}
                            sudo systemctl daemon-reload
                            sudo systemctl start ${SERVICE}
                            echo 'Service status:'
                            sudo systemctl status ${SERVICE} --no-pager | head -3
                        "
                    """
                }
            }
        }
        
        stage('Verify') {
            steps {
                sshagent(['deployment_key']) {
                    sh """
                        ssh ${USER}@${SERVER} "
                            if systemctl is-active ${SERVICE} >/dev/null; then
                                echo '✅ ${SERVICE} is running'
                                echo '📁 Files in ${DEPLOY_PATH}:'
                                ls -la ${DEPLOY_PATH}/ | grep -E '(.dll|appsettings)' | head -5
                            else
                                echo '❌ ${SERVICE} failed to start'
                                sudo journalctl -u ${SERVICE} -n 20 --no-pager
                                exit 1
                            fi
                        "
                    """
                }
            }
        }
    }
    
    post {
        success {
            echo '✅ .NET 10 APP deployed successfully!'
        }
        failure {
            echo '❌ Deployment failed'
        }
    }
}