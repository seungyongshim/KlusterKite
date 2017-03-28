import React from 'react';
import Relay from 'react-relay'

import './styles.css';

export class NodesWithTemplates extends React.Component {
  constructor(props) {
    super(props);
    this.drawTemplate = this.drawTemplate.bind(this);
  }

  static propTypes = {
    data: React.PropTypes.object,
  };

  drawTemplate(template) {
    const data = this.props.data.getActiveNodeDescriptions.edges;

    const nodesCount = data.filter(n => n.node.nodeTemplate === template.code).length;
    let color;
    if (nodesCount < template.minimumRequiredInstances) {
      color = 'label-danger';
    } else if (nodesCount === template.minimumRequiredInstances) {
      color = 'label-warning';
    } else {
      color = 'label-success';
    }

    return (
      <span title={template.name} className={`label ${color}`}>{template.code}: {nodesCount} / {template.minimumRequiredInstances}</span>
    );
  }

  render() {
    if (!this.props.data.getActiveNodeDescriptions){
      return (<div></div>);
    }
    const activeNodes = this.props.data.getActiveNodeDescriptions.edges;
    const templates = this.props.data.nodeTemplates && this.props.data.nodeTemplates.edges;

    return (
      <div className="templates">
        <div>
          <span className="label label-default">Total nodes: {(activeNodes && activeNodes.length) || 'unknown'}</span>
        </div>
        {templates && templates.map((template) =>
          <div key={template.node.code}>
            {this.drawTemplate(template.node)}
          </div>
        )}
      </div>
    );
  }
}

export default Relay.createContainer(
  NodesWithTemplates,
  {
    fragments: {
      data: () => Relay.QL`fragment on IClusterKitNodeApi_ClusterKitNodeManagement {
        getActiveNodeDescriptions {
          edges {
            node {
              nodeTemplate
            }
          }
        }
        nodeTemplates {
          edges {
            node {
              id
              code
              minimumRequiredInstances
              name
            }
          }
        }
      }
      `,
    },
  },
)
