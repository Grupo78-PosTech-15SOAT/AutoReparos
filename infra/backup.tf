# -------------------------------------------------------------
# IAM Role for AWS Data Lifecycle Manager (DLM)
# -------------------------------------------------------------
resource "aws_iam_role" "dlm_lifecycle" {
  name = "${var.cluster_name}-dlm-lifecycle-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "dlm.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name        = "${var.cluster_name}-dlm-lifecycle-role"
    Environment = var.environment
  }
}

resource "aws_iam_role_policy_attachment" "dlm_lifecycle" {
  role       = aws_iam_role.dlm_lifecycle.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSDataLifecycleManagerServiceRole"
}

# -------------------------------------------------------------
# AWS Data Lifecycle Manager (DLM) Lifecycle Policy
# -------------------------------------------------------------
resource "aws_dlm_lifecycle_policy" "ebs_backup" {
  description        = "Daily backup policy for EKS EBS volumes"
  execution_role_arn = aws_iam_role.dlm_lifecycle.arn
  state              = "ENABLED"

  policy_details {
    resource_types = ["VOLUME"]

    target_tags = {
      "kubernetes.io/cluster/${var.cluster_name}" = "owned"
    }

    schedule {
      name = "Daily Snapshots"

      create_rule {
        interval      = 24
        interval_unit = "HOURS"
        times         = ["03:00"] # 3:00 AM UTC (Midnight BRT)
      }

      retain_rule {
        count = 7 # Retain last 7 daily backups (1 week)
      }

      tags_to_add = {
        BackupCreator = "DataLifecycleManager"
        Cluster       = var.cluster_name
      }

      copy_tags = true
    }
  }

  tags = {
    Name        = "${var.cluster_name}-ebs-backup-policy"
    Environment = var.environment
  }
}
